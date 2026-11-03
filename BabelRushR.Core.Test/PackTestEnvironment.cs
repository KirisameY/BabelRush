using System.IO.Compression;
using System.Reflection;
using System.Text;

using BabelRushR.Core.Contents;
using BabelRushR.Core.Data;

using Zio;
using Zio.FileSystems;

namespace BabelRushR.Core.Test;

/// <summary>
///     用内存文件系统搭一个 contents 目录，供 <see cref="ContentsLoaderManager.LoadPacks"/> 的端到端用例使用。
/// </summary>
/// <remarks>
///     <see cref="ContentsLoaderManager"/> 是进程级静态状态，用例之间必须串行并复位；两个与它有关的测试类都放在
///     <c>Contents</c> collection 里，构造函数与 Dispose 各复位一次。
/// </remarks>
internal sealed class PackTestEnvironment : IDisposable
{
    private readonly MemoryFileSystem _fileSystem = new();

    public PackTestEnvironment()
    {
        ResetAutoLoaders();
        ResourceLoaderManagerAccessor.ResetAll();
        IsolateDataRegistration();
    }

    public DirectoryEntry Contents => new(_fileSystem, UPath.Root);

    /// <summary>绕开 <see cref="PackTestEnvironment"/> 已释放的包视图，直接查底层 contents 目录。</summary>
    public bool Exists(string path) => _fileSystem.FileExists(path) || _fileSystem.DirectoryExists(path);

    /// <summary>包的文件系统是否已经释放：未释放时 zip 文件句柄仍被占用，独占打开会失败。</summary>
    public bool IsUnlocked(string path)
    {
        try
        {
            using var _ = _fileSystem.OpenFile(new UPath(path), FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public PackTestEnvironment AddDirectoryPack(PackSpec pack)
    {
        var root = (UPath)"/" / pack.Id;
        WriteRaw((root / "pack.toml").ToString(), pack.MetaToml());
        foreach (var (path, bytes) in pack.Files)
        {
            WriteRaw((root / path).ToString(), bytes);
        }

        return this;
    }

    public PackTestEnvironment AddZipPack(PackSpec pack, string? fileName = null)
    {
        using var buffer = new MemoryStream();
        using (var archive = new ZipArchive(buffer, ZipArchiveMode.Create, leaveOpen: true))
        {
            WriteZipEntry(archive, "pack.toml", Encoding.UTF8.GetBytes(pack.MetaToml()));

            // ZipArchiveFileSystem 不会为隐式路径合成目录，而目录式注册（如 /cards）需要能命中目录项；
            // 常见打包工具也会写入目录项，这里照样补上。
            foreach (var directory in pack.Files.Keys.SelectMany(ParentDirectories).Distinct(StringComparer.Ordinal))
            {
                archive.CreateEntry(directory + "/");
            }

            foreach (var (path, bytes) in pack.Files)
            {
                WriteZipEntry(archive, path, bytes);
            }
        }

        WriteRaw($"/{fileName ?? $"{pack.Id}.zip"}", buffer.ToArray());
        return this;
    }

    public PackTestEnvironment WriteRaw(string path, string content) =>
        WriteRaw(path, Encoding.UTF8.GetBytes(content));

    public PackTestEnvironment WriteRaw(string path, byte[] bytes)
    {
        var filePath = new UPath(path);
        var directory = filePath.GetDirectory();
        EnsureDirectory(directory);
        _fileSystem.WriteAllBytes(filePath, bytes);
        return this;
    }

    public RecordingProgress Load()
    {
        var progress = new RecordingProgress();
        ContentsLoaderManager.LoadPacks(Contents, progress);
        return progress;
    }

    public void Dispose()
    {
        ResourceLoaderManagerAccessor.ResetAll();
        ResetAutoLoaders();
        _fileSystem.Dispose();
    }

    private static void ResetAutoLoaders()
    {
        AutoRecordingLoader.Reset();
        MultiPathAutoLoader.Reset();
    }

    /// <summary>
    ///     把数据层注册表的全局状态固定在「静态初始化已完成、<c>RegisterDone</c> 订阅已清空」。
    /// </summary>
    /// <remarks>
    ///     生产里 <see cref="ContentsLoaderManager.LoadPacks"/> 只会跑一次，`DataLoader.Dispose` 触发一次
    ///     <c>DataRegistrationManager.RegisterDone</c>；测试要在同一进程里反复加载，第二次触发会让已经冻结的
    ///     `CardType.Registry` 再次 Freeze，抛 `RegisterTargetUnavailableException`。
    ///     <para>
    ///         这里用反射先跑一次 <c>InitializeRegisters</c>（让 `CardType` 的静态构造与订阅发生），再把
    ///         <c>RegisterDone</c> 的订阅清空。之后每个用例都从「注册表未冻结」的同一状态开始，生产代码不需要
    ///         为测试让步。
    ///     </para>
    /// </remarks>
    private static void IsolateDataRegistration()
    {
        var managerType = typeof(DataRegistrationManager);
        var initialize = managerType.GetMethod("InitializeRegisters", BindingFlags.Static | BindingFlags.NonPublic)
         ?? throw new MissingMethodException(managerType.FullName, "InitializeRegisters");
        initialize.Invoke(null, null);

        var registerDone = managerType.GetField("RegisterDone", BindingFlags.Static | BindingFlags.NonPublic)
         ?? throw new MissingFieldException(managerType.FullName, "RegisterDone");
        registerDone.SetValue(null, null);
    }

    private void EnsureDirectory(UPath directory)
    {
        if (directory.IsNull || directory == UPath.Root || _fileSystem.DirectoryExists(directory)) return;
        EnsureDirectory(directory.GetDirectory());
        _fileSystem.CreateDirectory(directory);
    }

    private static void WriteZipEntry(ZipArchive archive, string path, byte[] bytes)
    {
        var entry = archive.CreateEntry(path);
        using var stream = entry.Open();
        stream.Write(bytes);
    }

    private static IEnumerable<string> ParentDirectories(string path)
    {
        var separator = path.LastIndexOf('/');
        while (separator > 0)
        {
            var directory = path[..separator];
            yield return directory;
            separator = directory.LastIndexOf('/');
        }
    }
}

internal sealed class PackSpec(string id)
{
    public string Id { get; } = id;
    public string Name { get; set; } = id;
    public string Version { get; set; } = "1.0.0";
    public List<(string Id, string Version)> Dependencies { get; } = [];
    public List<string> LoadBefore { get; } = [];
    public List<string> LoadAfter { get; } = [];
    public Dictionary<string, byte[]> Files { get; } = [];

    public PackSpec DependsOn(string packId, string version = "1.0.0")
    {
        Dependencies.Add((packId, version));
        return this;
    }

    public PackSpec Before(params string[] packIds)
    {
        LoadBefore.AddRange(packIds);
        return this;
    }

    public PackSpec After(params string[] packIds)
    {
        LoadAfter.AddRange(packIds);
        return this;
    }

    public PackSpec WithFile(string path, string content)
    {
        Files[path.TrimStart('/')] = Encoding.UTF8.GetBytes(content);
        return this;
    }

    public PackSpec WithFile(string path, byte[] bytes)
    {
        Files[path.TrimStart('/')] = bytes;
        return this;
    }

    public string MetaToml()
    {
        var builder = new StringBuilder();
        builder.AppendLine($"pack_id = \"{Id}\"");
        builder.AppendLine($"pack_name = \"{Name}\"");
        builder.AppendLine($"version = \"{Version}\"");

        AppendStringArray(builder, "load_before", LoadBefore);
        AppendStringArray(builder, "load_after", LoadAfter);

        foreach (var (dependencyId, dependencyVersion) in Dependencies)
        {
            builder.AppendLine();
            builder.AppendLine("[[dependencies]]");
            builder.AppendLine($"pack_id = \"{dependencyId}\"");
            builder.AppendLine($"version = \"{dependencyVersion}\"");
        }

        return builder.ToString();
    }

    private static void AppendStringArray(StringBuilder builder, string key, IReadOnlyCollection<string> values)
    {
        if (values.Count == 0) return;
        builder.AppendLine($"{key} = [{string.Join(", ", values.Select(value => $"\"{value}\""))}]");
    }
}

internal sealed class RecordingProgress : IProgress<IReadOnlyList<string>>
{
    public List<IReadOnlyList<string>> Reports { get; } = [];

    public void Report(IReadOnlyList<string> value) => Reports.Add([..value]);

    public bool Contains(params string[] steps) =>
        Reports.Any(report => report.Count == steps.Length && report.SequenceEqual(steps));

    public int IndexOf(params string[] steps) =>
        Reports.FindIndex(report => report.Count == steps.Length && report.SequenceEqual(steps));
}

internal sealed record LoadCall(string Package, UPath Path, FileSystemEntry Entry, bool IsDirectory)
{
    public string ReadText(string relativePath) => Entry.FileSystem.ReadAllText(Path / relativePath);
}

internal sealed class RecordingContentsLoader : IContentsLoader, IDisposable
{
    public List<LoadCall> Calls { get; } = [];
    public List<string> Events { get; } = [];
    public List<string> Contents { get; } = [];
    public int DisposeCount { get; private set; }

    /// <summary>相对当前 entry 读取的路径；在 Load 期间读，因为包的文件系统在加载结束后会被释放。</summary>
    public string? ReadRelativePath { get; set; }

    public void Load(string packageName, FileSystemEntry entry)
    {
        var call = new LoadCall(packageName, entry.Path, entry, entry is DirectoryEntry);
        Calls.Add(call);
        Events.Add($"load:{packageName}");

        if (ReadRelativePath is not null) Contents.Add(call.ReadText(ReadRelativePath));
    }

    public void Dispose()
    {
        DisposeCount++;
        Events.Add("dispose");
    }
}

/// <summary>
///     带 <see cref="AutoContentsLoaderAttribute"/> 的测试加载器：由 <see cref="ContentsLoaderManager.LoadPacks"/>
///     自动发现、实例化并派发，用来覆盖「扫描 → 自动注册」这条路径。
/// </summary>
[AutoContentsLoader("/auto")]
internal sealed class AutoRecordingLoader : IContentsLoader<AutoRecordingLoader>, IDisposable
{
    public static List<LoadCall> Calls { get; private set; } = [];
    public static List<string> Contents { get; private set; } = [];
    public static int CreateCount { get; private set; }
    public static int DisposeCount { get; private set; }

    public static AutoRecordingLoader Create()
    {
        CreateCount++;
        return new AutoRecordingLoader();
    }

    public static void Reset()
    {
        Calls = [];
        Contents = [];
        CreateCount = 0;
        DisposeCount = 0;
    }

    public void Load(string packageName, FileSystemEntry entry)
    {
        Calls.Add(new LoadCall(packageName, entry.Path, entry, entry is DirectoryEntry));

        // 读一下内容：如果包的文件系统已经被前一个加载器释放掉，这里会直接暴露出来。
        if (entry.FileSystem.FileExists(entry.Path / "marker.txt"))
        {
            Contents.Add(entry.FileSystem.ReadAllText(entry.Path / "marker.txt"));
        }
    }

    public void Dispose() => DisposeCount++;
}

/// <summary>一个类型标注两个路径：用来钉住「同一类型只创建一个实例，供所有路径复用」。</summary>
[AutoContentsLoader("/multi-a")]
[AutoContentsLoader("/multi-b")]
internal sealed class MultiPathAutoLoader : IContentsLoader<MultiPathAutoLoader>, IDisposable
{
    public static List<LoadCall> Calls { get; private set; } = [];
    public static int CreateCount { get; private set; }
    public static int DisposeCount { get; private set; }

    public static MultiPathAutoLoader Create()
    {
        CreateCount++;
        return new MultiPathAutoLoader();
    }

    public static void Reset()
    {
        Calls = [];
        CreateCount = 0;
        DisposeCount = 0;
    }

    public void Load(string packageName, FileSystemEntry entry) =>
        Calls.Add(new LoadCall(packageName, entry.Path, entry, entry is DirectoryEntry));

    public void Dispose() => DisposeCount++;
}
