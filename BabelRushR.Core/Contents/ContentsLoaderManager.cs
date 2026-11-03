using System.Collections.Immutable;
using System.IO.Compression;
using System.Reflection;

using BabelRushR.Core.Common;
using BabelRushR.Core.Common.Serializing;

using JetBrains.Annotations;

using KirisameY.GenericUtils;
using KirisameY.Relinq.Extensions;

using Tomlyn;

using Zio;
using Zio.FileSystems;

namespace BabelRushR.Core.Contents;

/// <summary>
///     维护「包内路径 → <see cref="IContentsLoader"/>」的注册表，并提供内容包的枚举入口。
/// </summary>
/// <remarks>
///     <para>
///         包内路径统一用 <see cref="UPath"/> 表示：以 <c>/</c> 开头、从包根（<see cref="UPath.Root"/>）算起的绝对路径，
///         例如 <c>/cards/strike.toml</c>。不用裸 <see cref="string"/>，是为了让归一化只存在一处实现——
///         <see cref="UPath"/> 的构造函数会把 <c>\</c> 归一为 <c>/</c>、去掉 <c>.</c> 段、消解 <c>..</c>、
///         合并重复分隔符并去掉结尾分隔符；注册时存下的路径因此和加载期拿到的路径不可能各用一套规则而对不上。
///     </para>
///     <para>
///         注册路径既可以是目录也可以是具体文件，<see cref="UPath.Root"/> 表示包根。
///         注册项按调用顺序保存在列表里：同一路径允许重复注册，后注册的项既不覆盖也不挤掉先前的项，
///         因为「哪个加载器最终生效」属于分发阶段的问题，注册表只负责如实记录。比较按序号进行，因此大小写敏感。
///     </para>
///     <para>
///         注意 <c>..</c> 越过包根的写法（如 <c>/a/../../b</c>）在构造 <see cref="UPath"/> 时就会抛
///         <see cref="ArgumentException"/>，管理器根本收不到这种值；管理器自己只拦「不是绝对路径」。
///     </para>
///     管理器只依赖 <see cref="IContentsLoader"/>；带静态工厂的是派生接口 <c>IResourceLoader&lt;TSelf&gt;</c>。
///     管理器是进程级静态状态：注册应在加载任何包之前完成，列表本身不做并发保护。
/// </remarks>
public static class ContentsLoaderManager
{
    #region Loader Register

    // 用列表而不是字典：同一路径允许注册多次，且注册顺序本身就是信息，字典的去重与无序性都不合适。
    // 元组不做校验，Path 的归一化与「必须是绝对路径」在 Register 里一次完成。
    private static readonly List<(UPath Path, IContentsLoader Loader)> Loaders = [];

    /// <summary>
    ///     把包内绝对路径 <paramref name="path"/> 注册给 <paramref name="loader"/>。
    /// </summary>
    /// <remarks>
    ///     注册按调用顺序追加，同一路径可以注册多次；本方法不去重、不覆盖、也不检查是否已有加载器覆盖该路径。
    ///     「同一路径上的多个加载器谁生效」留给分发阶段决定，注册表不替调用方做选择。
    /// </remarks>
    /// <param name="path">从包根开始的绝对路径，可以是目录或具体文件；<see cref="UPath.Root"/> 表示包根。</param>
    /// <param name="loader">负责该路径及其后代路径的加载器。</param>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> 为 <c>null</c>，或 <paramref name="path"/> 是 null 路径。</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> 不是从包根开始的绝对路径。</exception>
    public static void Register(UPath path, IContentsLoader loader)
    {
        if (_loaded) throw new InvalidOperationException("ResourceLoader already loaded content packs.");
        ArgumentNullException.ThrowIfNull(loader);

        Loaders.Add((ValidatePath(path), loader));
    }

    /// <summary>
    ///     发现标注了 <see cref="AutoContentsLoaderAttribute"/> 的加载器类型，实例化并按特性上的路径注册。
    /// </summary>
    /// <remarks>
    ///     尚未接线：等包的遍历完成后由加载流程调用。同一类型只创建一个实例，供它的所有路径复用；
    ///     本方法不幂等，重复调用会让每个路径多出一份注册，只应调用一次。
    /// </remarks>
    private static void RegisterAttributedLoaders()
    {
        var loaderTypes = AttributedTypeScanner.FindTypesWithAttribute<AutoContentsLoaderAttribute>();
        foreach (var loaderType in loaderTypes)
        {
            ValidateLoaderType(loaderType);
        }

        foreach (var loaderType in loaderTypes)
        {
            var loader = CreateLoader(loaderType);
            foreach (var attribute in loaderType.GetCustomAttributes<AutoContentsLoaderAttribute>(inherit: false))
            {
                Register(attribute.Path, loader);
            }
        }
    }

    // 仅供测试通过反射调用；不提升到 internal，避免测试细节进入程序集契约。
    [UsedImplicitly] private static void ClearRegistrations() => Loaders.Clear();

    #endregion


    #region Register Utils

    /// <summary>
    ///     确认 <paramref name="path"/> 是包内路径，并原样返回它。
    /// </summary>
    /// <remarks>
    ///     这里只检查「是不是从包根开始的绝对路径」；分隔符、<c>.</c> 与 <c>..</c> 的归一化已经由
    ///     <see cref="UPath"/> 的构造函数完成。相对路径视为调用错误而不是自动补 <c>/</c>：包内路径的约定是绝对的，
    ///     静默转换只会把写错的注册拖到运行期才暴露。
    /// </remarks>
    private static UPath ValidatePath(UPath path)
    {
        path.AssertNotNull();

        if (!path.IsAbsolute)
        {
            throw new ArgumentException(
                $"Package path '{path}' must be absolute, starting with '/' from the package root (e.g. '/cards').",
                nameof(path));
        }

        return path;
    }

    private static void ValidateLoaderType(Type loaderType)
    {
        AttributedTypeScanner.ValidateRegistrationType(loaderType, typeof(IContentsLoader), "Resource loader");

        // 自动实例化要求实现的是 IResourceLoader<自己>，而不是别的 TSelf。
        // 不能用 MakeGenericType 来检查：约束不满足时它自己就会抛 ArgumentException。
        var implementsSelfFactory = loaderType.GetInterfaces().Any(implemented =>
                                                                       implemented.IsGenericType
                                                                    && implemented.GetGenericTypeDefinition() == typeof(IContentsLoader<>)
                                                                    && implemented.GetGenericArguments()[0] == loaderType);
        if (!implementsSelfFactory)
        {
            throw new InvalidOperationException(
                $"Resource loader '{loaderType.FullName}' must implement IResourceLoader<{loaderType.Name}>.");
        }
    }

    private static IContentsLoader CreateLoader(Type loaderType)
    {
        try
        {
            var method = typeof(ContentsLoaderManager)
                        .GetMethod(nameof(CreateLoaderCore), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(loaderType);
            return method.Invoke(null, null) as IContentsLoader
             ?? throw new InvalidOperationException(
                    $"Resource loader '{loaderType.FullName}' returned null from IResourceLoader<{loaderType.Name}>.Create().");
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create resource loader '{loaderType.FullName}'.", ex.InnerException ?? ex);
        }
    }

    private static TSelf CreateLoaderCore<TSelf>() where TSelf : IContentsLoader<TSelf> => TSelf.Create();

    #endregion


    #region Scan Packs

    private static bool _loaded = false;

    public static Task LoadPacksAsync(DirectoryEntry contentsDir, IProgress<IReadOnlyList<string>> progress) =>
        Task.Run(() => LoadPacks(contentsDir, progress));

    public static void LoadPacks(DirectoryEntry contentsDir, IProgress<IReadOnlyList<string>> progress)
    {
        if (Interlocked.Exchange(ref _loaded, true)) throw new InvalidOperationException("ResourceLoader already loaded content packs.");

        progress.Report(["solving_content_packs"]);
        var entries = contentsDir.EnumerateEntries();
        // 目前只硬编码适配目录和zip包，哪天有需要的话可以写个接口支持更多格式的拓展不过我觉得很难会有这个必要
        var packs = entries.SelectExist(Optional<(string Path, IFileSystem Fs)> (entry) => entry switch
        {
            DirectoryEntry directory => Optional.From((directory.Name, new SubFileSystem(contentsDir.FileSystem, directory.Path) as IFileSystem)),
            FileEntry { ExtensionWithDot: ".zip" } file => Optional.From(
                (file.NameWithoutExtension,
                 new ZipArchiveFileSystem(file.Open(FileMode.Open, FileAccess.Read), ZipArchiveMode.Read) as IFileSystem)
            ),
            _ => Optional.Empty<(string, IFileSystem)>()
        }).ToImmutableList();
        var packsWithMeta = ResolveMetaData(packs);

        progress.Report(["validating_dependencies"]);
        var packsSortedAsDependencies = ValidateDependencies(packsWithMeta);

        progress.Report(["loading_assemblies"]);
        LoadPackAssemblies(packsSortedAsDependencies);

        progress.Report(["solving_load_order"]);
        var sortedPacks = ResolveOrder(packsWithMeta);

        progress.Report(["scanning_loaders"]);
        RegisterAttributedLoaders();

        // loading resources
        foreach (var (_, packFs, meta) in sortedPacks)
        {
            progress.Report(["loading_pack_resources", meta.PackId]);
            foreach (var (path, loader) in Loaders)
            {
                if (packFs.TryGetFileSystemEntry(path) is not { } entry) continue;
                loader.Load(meta.PackId, entry);
            }
        }

        progress.Report(["finalizing"]);
        foreach (var t in packs) t.Fs.Dispose();
        foreach (var loader in Loaders.Select(t => t.Loader).OfType<IDisposable>())
        {
            loader.Dispose();
        }
    }

    private static void LoadPackAssemblies(ImmutableList<(string Path, IFileSystem Fs, PackMetaData Meta)> packs)
    {
        Dictionary<string, PackAssemblyLoadContext> contexts = [];
        var dllPath = (UPath)"/$assemblies";
        foreach (var (_, fs, meta) in packs)
        {
            var ctx = contexts[meta.PackId] = new PackAssemblyLoadContext(
                meta.PackId, [..meta.Dependencies.Select(d => contexts[d.PackId])]
            );
            foreach (var file in fs.EnumerateFileEntries(dllPath, "*.dll", SearchOption.AllDirectories))
            {
                using var stream = file.Open(FileMode.Open, FileAccess.Read);
                ctx.LoadFromStream(stream);
            }
        }
    }

    #endregion


    #region Pack Utils

    private static ImmutableList<(string Path, IFileSystem Fs, PackMetaData Meta)> ResolveMetaData(
        IReadOnlyList<(string Path, IFileSystem Fs)> packs)
    {
        var metaPath = (UPath)"/pack.toml";
        List<(string Path, IFileSystem Fs, PackMetaData Meta)> result = new(capacity: packs.Count);
        foreach (var (path, fs) in packs)
        {
            if (!fs.FileExists(metaPath))
            {
                // todo: 在这里报个警告
                continue;
            }
            using var metaStream = fs.GetFileEntry(metaPath).Open(FileMode.Open, FileAccess.Read);
            var meta = TomlSerializer.Deserialize<PackMetaData>(metaStream, SerializingOptions.Default)!; // todo: 异常处理+警告+跳过
            result.Add((path, fs, meta));
        }
        return [..result];
    }

    private static ImmutableList<(string Path, IFileSystem Fs, PackMetaData Meta)> ValidateDependencies(
        IReadOnlyList<(string Path, IFileSystem Fs, PackMetaData Meta)> packs)
    {
        var unfoundDependencies = packs.Select( //
            l => l.Meta
        ).Where( //
            m => !m.Dependencies.IsEmpty
        ).SelectMany( //
            m => m.Dependencies.Select(dep => (depId: dep.PackId, depV: dep.Version, id: m.PackId))
        ).GroupBy(
            t => t.depId,
            (depId, ts) => KeyValuePair.Create(
                depId, ts.Select(t => (t.depV, t.id)).ToList()
            )
        ).ToDictionary();

        List<string> errors = [];

        foreach (var (_, _, packMeta) in packs)
        {
            if (!unfoundDependencies.TryGetValue(packMeta.PackId, out var l)) continue;
            l.RemoveAll(t => t.depV <= packMeta.Version);
            if (l.Count != 0)
            {
                foreach (var (depV, id) in l)
                {
                    var msg = $"{id} need {packMeta.PackId} not lower than {depV}, but current {packMeta.PackId} version is {packMeta.Version}";
                    errors.Add(msg);
                }
            }
            unfoundDependencies.Remove(packMeta.PackId);
        }
        foreach (var (depId, depV, id) in
                 unfoundDependencies.SelectMany(kvp => kvp.Value.Select(t => (depId: kvp.Key, t.depV, t.id))))
        {
            var msg = $"{id} need {depId} not lower than {depV}, but {depId} not found.";
            errors.Add(msg);
        }

        if (errors.Count > 0) throw new PackDependenciesNotSatisfiedException(errors);

        var dependencies = packs.Select(t => t.Meta)
                                .Where(meta => !meta.Dependencies.IsEmpty)
                                .ToDictionary(
                                     meta => meta.PackId,
                                     meta => meta.Dependencies.Select(dep => dep.PackId).Distinct().ToImmutableList()
                                 );

        return SortByDependencies(
            packs,
            t => t.Meta.PackId,
            dependencies,
            unsolvedIds => new PackCircularDependencyException(unsolvedIds.Select(id =>
            {
                return $"pack [{id}] depends on: {dependencies[id].Select(dep => $"\n    {dep}").Join(',')}\n";
            }))
        );
    }

    private static ImmutableList<(string Path, IFileSystem Fs, PackMetaData Meta)> ResolveOrder(
        IReadOnlyList<(string Path, IFileSystem Fs, PackMetaData Meta)> packs)
    {
        var orderCells = packs.Select(t => t.Meta).SelectMany(meta =>
        {
            // ReSharper disable once InvokeAsExtensionMember
            return Enumerable.Concat(
                meta.LoadAfter.Select(before => (Before: before, After: meta.PackId)),
                meta.LoadBefore.Select(after => (Before: meta.PackId, After: after))
            );
        }).GroupBy(
            t => t.After,
            (after, orders) => KeyValuePair.Create(after, orders.Select(t => t.Before).ToImmutableList())
        ).ToImmutableDictionary();

        var packsById = packs.ToDictionary(t => t.Meta.PackId);

        return SortByDependencies(
            packs,
            t => t.Meta.PackId,
            orderCells,
            unsolvedIds => new PackCircularOrderedException(unsolvedIds.Select(id => $"""
            pack [{id}] should be loaded 
              after: {packsById[id].Meta.LoadAfter.Select(before => $"\n    {before}").Join(',')}
              and before: {packsById[id].Meta.LoadBefore.Select(after => $"\n    {after}").Join(',')}

            """))
        );
    }

    /// <summary>
    ///     按 <paramref name="dependencies"/> 给出的前置关系对 <paramref name="packs"/> 做拓扑排序，并保持同层元素的原始顺序。
    /// </summary>
    /// <remarks>
    ///     <paramref name="dependencies"/> 的键是等待方，值是要先于它出现的包 Id；指向不存在的包的依赖视为已满足。
    ///     排序结束后仍有包排不进去，说明依赖成环，此时用 <paramref name="createCircularException"/> 构造并抛出异常。
    /// </remarks>
    private static ImmutableList<T> SortByDependencies<T>(
        IReadOnlyList<T> packs,
        Func<T, string> idSelector,
        IReadOnlyDictionary<string, ImmutableList<string>> dependencies,
        Func<IReadOnlyCollection<string>, Exception> createCircularException)
    {
        Dictionary<string, Action> pendingDelegates = [];
        List<T> orderedPacks = new(capacity: packs.Count);
        int pointer = 0;

        // traverse all packs and init state
        foreach (var t in packs)
        {
            var id = idSelector(t);
            if (!dependencies.TryGetValue(id, out var deps) || deps.IsEmpty)
            {
                orderedPacks.Add(t);
            }
            else
            {
                var left = deps.Count;
                var pend = () =>
                {
                    left--;
                    if (left == 0) orderedPacks.Add(t);
                };
                foreach (var dep in deps)
                {
                    var current = pendingDelegates.GetValueOrDefault(dep);
                    pendingDelegates[dep] = current + pend;
                }
            }
        }

        // treat those not exist packages as handled
        var unsolvedIds = packs.Select(idSelector).ToHashSet();
        foreach (var (dep, act) in pendingDelegates)
        {
            if (!unsolvedIds.Contains(dep)) act.Invoke();
        }

        // now solve the rest
        while (pointer < orderedPacks.Count)
        {
            var id = idSelector(orderedPacks[pointer]);
            pendingDelegates.GetValueOrDefault(id)?.Invoke();
            unsolvedIds.Remove(id);
            pointer++;
        }

        // not all packs resolved means there's circular dependencies
        return unsolvedIds.Count == 0
            ? [..orderedPacks]
            : throw createCircularException(unsolvedIds);
    }

    #endregion
}