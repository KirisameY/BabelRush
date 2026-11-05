using System.Runtime.Loader;

using BabelRushR.Core.Contents;

using Zio;

namespace BabelRushR.Core.Test;

[CollectionDefinition("Contents", DisableParallelization = true)]
public sealed class ContentsCollection;

/// <summary>
///     <see cref="ContentsLoaderManager.LoadPacks"/> 的端到端用例：在内存文件系统里搭包，观察加载器收到什么。
/// </summary>
/// <remarks>
///     断言集中在「对外可观察的行为」：哪些包被加载、按什么顺序、加载器收到哪个 entry、异常类型与进度步骤。
///     内部实现（例如拓扑排序怎么记账）不在这里钉。
/// </remarks>
[Collection("Contents")]
public class PackLoadingTests
{
    // ── 包发现与派发 ──────────────────────────────────────────────

    [Fact]
    public void LoadPacks_Dispatches_A_Directory_Pack_To_The_Registered_Loader()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("alpha").WithFile("cards/strike.toml", "name = \"strike\""));
        var loader = new RecordingContentsLoader { ReadRelativePath = "strike.toml" };
        ContentsLoaderManager.Register("/cards", loader);

        env.Load();

        var call = Assert.Single(loader.Calls);
        Assert.Equal("alpha", call.Package);
        Assert.Equal(new UPath("/cards"), call.Path);
        Assert.True(call.IsDirectory);
        // 加载器拿到的 entry 以包根为根，可以直接按注册路径下的相对路径读文件。
        Assert.Equal("name = \"strike\"", Assert.Single(loader.Contents));
    }

    [Fact]
    public void LoadPacks_Dispatches_A_File_Entry_When_The_Registration_Names_A_File()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("alpha").WithFile("info.txt", "hello"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/info.txt", loader);

        // 注册到文件本身时，派发的是 FileEntry 而不是它所在的目录。
        env.Load();

        var call = Assert.Single(loader.Calls);
        Assert.False(call.IsDirectory);
        Assert.Equal(new UPath("/info.txt"), call.Path);
    }

    [Fact]
    public void LoadPacks_Supports_Zip_Packs()
    {
        using var env = new PackTestEnvironment();
        env.AddZipPack(new PackSpec("zipped").WithFile("cards/z.toml", "zipped = true"));
        var loader = new RecordingContentsLoader { ReadRelativePath = "z.toml" };
        ContentsLoaderManager.Register("/cards", loader);

        env.Load();

        var call = Assert.Single(loader.Calls);
        Assert.Equal("zipped", call.Package);
        Assert.True(call.IsDirectory);
        Assert.Equal("zipped = true", Assert.Single(loader.Contents));
    }

    [Fact]
    public void LoadPacks_Recognizes_The_Zip_Extension_Case_Insensitively()
    {
        using var env = new PackTestEnvironment();
        env.AddZipPack(new PackSpec("upper").WithFile("cards/z.toml", "upper = true"), "UPPER.ZIP");
        var loader = new RecordingContentsLoader { ReadRelativePath = "z.toml" };
        ContentsLoaderManager.Register("/cards", loader);

        // Windows 上的扩展名大小写不应影响识别。
        env.Load();

        Assert.Single(loader.Calls);
        Assert.Equal("upper = true", Assert.Single(loader.Contents));
    }

    [Fact]
    public void LoadPacks_Skips_Entries_That_Are_Not_Packs()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("valid").WithFile("marker.txt", "ok"));
        env.WriteRaw("/readme.txt", "not a pack");
        env.WriteRaw("/no_meta/marker.txt", "also not a pack");

        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/", loader);

        env.Load();

        // 目录缺 pack.toml、根目录下的普通文件都不算包：只有 valid 被派发。
        Assert.Equal(["valid"], loader.Calls.Select(call => call.Package));
    }

    [Fact]
    public void LoadPacks_Does_Not_Invoke_A_Loader_Whose_Path_Is_Absent()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("alpha").WithFile("cards/a.toml", "a = 1"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/missing", loader);

        env.Load();

        Assert.Empty(loader.Calls);
        // 没被调用过的加载器仍然属于本次加载的生命周期，结束时统一释放。
        Assert.Equal(1, loader.DisposeCount);
    }

    // ── 加载顺序 ─────────────────────────────────────────────────

    [Fact]
    public void LoadPacks_Respects_LoadAfter()
    {
        using var env = new PackTestEnvironment();
        // 先加入 aaa：若没有 load_after，枚举顺序会让它先加载。
        env.AddDirectoryPack(new PackSpec("aaa").After("zzz").WithFile("marker.txt", "a"));
        env.AddDirectoryPack(new PackSpec("zzz").WithFile("marker.txt", "z"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/", loader);

        env.Load();

        Assert.Equal(["zzz", "aaa"], loader.Calls.Select(call => call.Package));
    }

    [Fact]
    public void LoadPacks_Respects_LoadBefore()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("zzz").Before("aaa").WithFile("marker.txt", "z"));
        env.AddDirectoryPack(new PackSpec("aaa").WithFile("marker.txt", "a"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/", loader);

        env.Load();

        Assert.Equal(["zzz", "aaa"], loader.Calls.Select(call => call.Package));
    }

    [Fact]
    public void LoadPacks_Throws_When_Load_Order_Rules_Form_A_Cycle()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("aaa").After("bbb"));
        env.AddDirectoryPack(new PackSpec("bbb").After("aaa"));

        var ex = Assert.Throws<PackCircularOrderedException>(() => env.Load());

        Assert.Contains("aaa", ex.Message);
        Assert.Contains("bbb", ex.Message);
    }

    [Fact]
    public void LoadPacks_Ignores_Order_Rules_Pointing_At_Packs_That_Are_Not_Present()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("alpha").After("not-installed").Before("also-not-installed"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/", loader);

        // 指向不存在的包不是错误，也不能把 alpha 卡在「等待一个永远不会出现的包」上。
        env.Load();

        Assert.Equal(["alpha"], loader.Calls.Select(call => call.Package));
    }

    [Fact]
    public void LoadPacks_Does_Not_Mistake_A_Duplicated_Order_Rule_For_A_Cycle()
    {
        using var env = new PackTestEnvironment();
        // 两边各写一次同一条关系：aaa 在 bbb 之后，bbb 在 aaa 之前。
        env.AddDirectoryPack(new PackSpec("aaa").After("bbb"));
        env.AddDirectoryPack(new PackSpec("bbb").Before("aaa"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/", loader);

        env.Load();

        Assert.Equal(["bbb", "aaa"], loader.Calls.Select(call => call.Package));
    }

    // ── 依赖 ─────────────────────────────────────────────────────

    [Fact]
    public void LoadPacks_Accepts_A_Satisfied_Dependency()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("base") { Version = "1.2.0" });
        env.AddDirectoryPack(new PackSpec("app").DependsOn("base", "1.2.0").WithFile("marker.txt", "a"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/", loader);

        env.Load();

        // 依赖满足即可加载；依赖顺序只用于 Assembly 上下文，不在这里额外规定资源派发顺序。
        Assert.Equal(2, loader.Calls.Count);
    }

    [Fact]
    public void LoadPacks_Throws_When_A_Dependency_Is_Missing()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("app").DependsOn("missing", "1.0.0"));

        var ex = Assert.Throws<PackDependenciesNotSatisfiedException>(() => env.Load());

        Assert.Contains("app", ex.Message);
        Assert.Contains("missing", ex.Message);
    }

    [Fact]
    public void LoadPacks_Throws_When_A_Dependency_Version_Is_Too_Low()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("base") { Version = "1.0.0" });
        env.AddDirectoryPack(new PackSpec("app").DependsOn("base", "1.5.0"));

        var ex = Assert.Throws<PackDependenciesNotSatisfiedException>(() => env.Load());

        Assert.Contains("app", ex.Message);
        Assert.Contains("base", ex.Message);
        Assert.Contains("1.5.0", ex.Message);
    }

    [Fact]
    public void LoadPacks_Throws_When_Dependencies_Form_A_Cycle()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("aaa").DependsOn("bbb"));
        env.AddDirectoryPack(new PackSpec("bbb").DependsOn("aaa"));

        var ex = Assert.Throws<PackCircularDependencyException>(() => env.Load());

        Assert.Contains("aaa", ex.Message);
        Assert.Contains("bbb", ex.Message);
    }

    // ── 生命周期与一次性状态 ─────────────────────────────────────

    [Fact]
    public void LoadPacks_Disposes_The_Registered_Loaders_After_Dispatching()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("alpha").WithFile("marker.txt", "ok"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/", loader);

        env.Load();

        Assert.Equal(1, loader.DisposeCount);
        Assert.Equal("dispose", loader.Events[^1]);
        Assert.Single(loader.Events.FindAll(evt => evt == "dispose"));
    }

    [Fact]
    public void LoadPacks_Releases_Pack_Files_And_Loaders_When_Loading_Fails()
    {
        using var env = new PackTestEnvironment();
        env.AddZipPack(new PackSpec("broken").DependsOn("missing"));
        var loader = new RecordingContentsLoader();
        ContentsLoaderManager.Register("/missing", loader);

        Assert.Throws<PackDependenciesNotSatisfiedException>(() => env.Load());

        // 包的 zip 句柄必须关掉，否则错误修复后重开游戏前文件一直被占用。
        Assert.True(env.IsUnlocked("/broken.zip"));
        // 已登记的加载器也属于这次失败的加载会话，应当释放。
        Assert.Equal(1, loader.DisposeCount);
    }

    [Fact]
    public void LoadPacks_Does_Not_Dispose_The_Contents_File_System()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("alpha").WithFile("marker.txt", "ok"));
        env.AddDirectoryPack(new PackSpec("beta").WithFile("marker.txt", "ok"));

        env.Load();

        // 目录包是 contents 目录的子视图；释放这些视图不能把调用方的整个 contents 目录一起废掉，
        // 否则第二个目录包、甚至加载结束后的任何访问都会炸。
        Assert.True(env.Exists("/alpha/pack.toml"));
        Assert.True(env.Exists("/beta/pack.toml"));
    }

    [Fact]
    public void LoadPacks_Cannot_Run_Twice()
    {
        using var env = new PackTestEnvironment();
        env.Load();

        var ex = Assert.Throws<InvalidOperationException>(() => env.Load());
        Assert.Contains("already", ex.Message);
    }

    [Fact]
    public void LoadPacks_Succeeds_With_No_Packs_At_All()
    {
        using var env = new PackTestEnvironment();

        var progress = env.Load();

        Assert.Equal(["finalizing"], progress.Reports[^1]);
    }

    [Fact]
    public void Register_Is_Rejected_After_A_Load()
    {
        using var env = new PackTestEnvironment();
        env.Load();

        Assert.Throws<InvalidOperationException>(() =>
                                                    ContentsLoaderManager.Register("/late", new RecordingContentsLoader()));
    }

    // ── 进度与自动注册 ───────────────────────────────────────────

    [Fact]
    public void LoadPacks_Reports_The_Loading_Steps_In_Order()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("alpha").WithFile("marker.txt", "ok"));

        var progress = env.Load();

        Assert.Equal(["solving_content_packs"], progress.Reports[0]);
        Assert.Equal(["finalizing"], progress.Reports[^1]);
        Assert.True(progress.Contains("validating_dependencies"));
        Assert.True(progress.Contains("loading_assemblies"));
        Assert.True(progress.Contains("solving_load_order"));
        Assert.True(progress.Contains("scanning_loaders"));
        Assert.True(progress.Contains("loading_pack_data", "alpha"));

        // 主阶段必须按依赖验证 → Assembly → 顺序解析 → 扫描加载器 → 逐包加载的次序报告。
        var steps = new[]
        {
            progress.IndexOf("validating_dependencies"),
            progress.IndexOf("loading_assemblies"),
            progress.IndexOf("solving_load_order"),
            progress.IndexOf("scanning_loaders"),
            progress.IndexOf("loading_pack_data", "alpha"),
            progress.IndexOf("finalizing"),
        };
        Assert.All(steps, index => Assert.True(index >= 0));
        Assert.Equal(steps.Order().ToArray(), steps);
    }

    [Fact]
    public void LoadPacks_Discovers_Attributed_Loaders_And_Dispatches_To_Them()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("auto-pack").WithFile("auto/marker.txt", "auto"));

        env.Load();

        // 带 [AutoContentsLoader] 的类型在 LoadPacks 内部被扫描、实例化并注册（此时管理器已进入「加载中」状态）。
        Assert.True(AutoRecordingLoader.CreateCount >= 1);
        Assert.Contains(AutoRecordingLoader.Calls,
                        call => call.Package == "auto-pack" && call.Path == new UPath("/auto"));
        Assert.True(AutoRecordingLoader.DisposeCount >= 1);
    }

    [Fact]
    public void LoadPacks_Keeps_A_Pack_Usable_For_Later_Loaders_In_The_Same_Pack()
    {
        using var env = new PackTestEnvironment();
        // /data 由 Core 的 DataLoader 处理；测试程序集里的 /auto 加载器排在它之后。两者作用于同一个包。
        env.AddDirectoryPack(new PackSpec("multi-loader")
                           .WithFile("data/placeholder.txt", "")
                           .WithFile("auto/marker.txt", "auto"));

        env.Load();

        Assert.Contains(AutoRecordingLoader.Calls,
                        call => call.Package == "multi-loader" && call.Path == new UPath("/auto"));
        Assert.Contains("auto", AutoRecordingLoader.Contents);
    }

    [Fact]
    public void LoadPacks_Reuses_One_Attributed_Loader_Instance_For_All_Its_Paths()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("multi-path")
                           .WithFile("multi-a/a.txt", "a")
                           .WithFile("multi-b/b.txt", "b"));

        env.Load();

        Assert.Equal(1, MultiPathAutoLoader.CreateCount);
        var paths = MultiPathAutoLoader.Calls.Where(call => call.Package == "multi-path")
                                        .Select(call => call.Path)
                                        .ToHashSet();
        Assert.Equal(2, paths.Count);
        Assert.Contains(new UPath("/multi-a"), paths);
        Assert.Contains(new UPath("/multi-b"), paths);
    }

    [Fact]
    public void LoadPacks_Disposes_A_Loader_Registered_For_Multiple_Paths_Only_Once()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("multi-path")
                           .WithFile("multi-a/a.txt", "a")
                           .WithFile("multi-b/b.txt", "b"));

        env.Load();

        // 一个实例注册了两条路径，释放生命周期仍然只有一次。
        Assert.Equal(1, MultiPathAutoLoader.CreateCount);
        Assert.Equal(1, MultiPathAutoLoader.DisposeCount);
    }

    [Fact]
    public async Task LoadPacksAsync_Loads_Content_Off_The_Calling_Thread()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("async-pack").WithFile("cards/a.toml", "a = 1"));
        var loader = new RecordingContentsLoader { ReadRelativePath = "a.toml" };
        ContentsLoaderManager.Register("/cards", loader);

        var progress = new RecordingProgress();
        await ContentsLoaderManager.LoadPacksAsync(env.Contents, progress);

        Assert.Single(loader.Calls);
        Assert.Equal("a = 1", Assert.Single(loader.Contents));
        Assert.Equal(["finalizing"], progress.Reports[^1]);
    }

    // ── 程序集加载 ───────────────────────────────────────────────

    [Fact]
    public void LoadPacks_Does_Not_Require_An_Assemblies_Directory()
    {
        using var env = new PackTestEnvironment();
        env.AddDirectoryPack(new PackSpec("plain").WithFile("cards/a.toml", "a = 1"));
        var loader = new RecordingContentsLoader { ReadRelativePath = "a.toml" };
        ContentsLoaderManager.Register("/cards", loader);

        // 带脚本/资源的普通包没有 $assemblies 目录，不能被当成错误。
        env.Load();

        Assert.Single(loader.Calls);
    }

    [Fact]
    public void LoadPacks_Loads_Dlls_From_The_Assemblies_Directory()
    {
        using var env = new PackTestEnvironment();
        const string assemblyName = "Fixture.PackAssembly.Direct";
        env.AddDirectoryPack(new PackSpec("asm-direct")
                           .WithFile("$assemblies/Fixture.dll",
                                     TestAssemblyBuilder.BuildFixtureAssembly(assemblyName)));

        env.Load();

        var context = FindPackContext("asm-direct");
        Assert.Contains(context.Assemblies, assembly => assembly.GetName().Name == assemblyName);
    }

    [Fact]
    public void LoadPacks_Loads_Dlls_Recursively_Below_The_Assemblies_Directory()
    {
        using var env = new PackTestEnvironment();
        const string assemblyName = "Fixture.PackAssembly.Nested";
        env.AddDirectoryPack(new PackSpec("asm-nested")
                           .WithFile("$assemblies/libs/Fixture.dll",
                                     TestAssemblyBuilder.BuildFixtureAssembly(assemblyName)));

        env.Load();

        var context = FindPackContext("asm-nested");
        Assert.Contains(context.Assemblies, assembly => assembly.GetName().Name == assemblyName);
    }

    private static AssemblyLoadContext FindPackContext(string packId)
    {
        var expectedName = $"PackAsmCtx({packId})";
        return Assert.Single(AssemblyLoadContext.All, context => context.Name == expectedName);
    }
}
