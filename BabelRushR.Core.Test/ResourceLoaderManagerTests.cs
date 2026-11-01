using System.Reflection;
using System.Runtime.ExceptionServices;

using BabelRushR.Core.Common;
using BabelRushR.Core.Resources;

using Zio;

namespace BabelRushR.Core.Test;

public class ResourceLoaderManagerTests
{
    public ResourceLoaderManagerTests() => ResourceLoaderManagerAccessor.InvokePrivateStatic("ClearRegistrations");


    [Fact]
    public void Register_Exact_File_Is_Found_By_Its_Path()
    {
        var loader = new RecordingLoader();

        ResourceLoaderManager.Register("/cards/strike.toml", loader);

        Assert.Same(loader, ResourceLoaderManager.FindLoader("/cards/strike.toml"));
        Assert.Null(ResourceLoaderManager.FindLoader("/cards/defend.toml"));
        Assert.Null(ResourceLoaderManager.FindLoader("/cards"));
        Assert.Null(ResourceLoaderManager.FindLoader("/"));
    }

    [Fact]
    public void FindLoader_Returns_The_Most_Specific_Registration()
    {
        var directoryLoader = new RecordingLoader();
        var nestedLoader = new RecordingLoader();

        ResourceLoaderManager.Register("/a/b", directoryLoader);
        ResourceLoaderManager.Register("/a/b/c", nestedLoader);

        // 分段前缀匹配：/a/b/c 及其后代命中后者，/a/b 的其它后代仍归前者。
        Assert.Same(nestedLoader, ResourceLoaderManager.FindLoader("/a/b/c"));
        Assert.Same(nestedLoader, ResourceLoaderManager.FindLoader("/a/b/c/d/e.toml"));
        Assert.Same(directoryLoader, ResourceLoaderManager.FindLoader("/a/b/d"));
        Assert.Same(directoryLoader, ResourceLoaderManager.FindLoader("/a/b"));
        Assert.Null(ResourceLoaderManager.FindLoader("/a"));
    }

    [Fact]
    public void Register_And_Find_Normalize_Separators_And_Dot_Segments()
    {
        var loader = new RecordingLoader();

        ResourceLoaderManager.Register(new UPath(@"\a\b\.\c\"), loader);

        Assert.Same(loader, ResourceLoaderManager.FindLoader("/a/b/c"));
        Assert.Same(loader, ResourceLoaderManager.FindLoader("/a//b/c/"));
        Assert.Same(loader, ResourceLoaderManager.FindLoader("/a/b/x/../c"));
    }

    [Fact]
    public void Root_Registration_Is_The_Fallback()
    {
        var fallbackLoader = new RecordingLoader();
        var cardsLoader = new RecordingLoader();

        ResourceLoaderManager.Register(UPath.Root, fallbackLoader);
        ResourceLoaderManager.Register("/cards", cardsLoader);

        Assert.Same(cardsLoader, ResourceLoaderManager.FindLoader("/cards/strike.toml"));
        Assert.Same(fallbackLoader, ResourceLoaderManager.FindLoader("/audio/bgm.ogg"));
        Assert.Same(fallbackLoader, ResourceLoaderManager.FindLoader("/"));
    }

    [Fact]
    public void FindLoader_Is_Case_Sensitive()
    {
        ResourceLoaderManager.Register("/Cards", new RecordingLoader());

        Assert.Null(ResourceLoaderManager.FindLoader("/cards"));
    }

    [Fact]
    public void Register_Rejects_Duplicate_Normalized_Path()
    {
        ResourceLoaderManager.Register("/a/b", new RecordingLoader());

        var ex = Assert.Throws<InvalidOperationException>(() =>
                                                              ResourceLoaderManager.Register(new UPath("/a//b/"), new RecordingLoader()));

        Assert.Contains("/a/b", ex.Message);
    }

    [Fact]
    public void Register_Rejects_Relative_Path()
    {
        // 包内路径的约定是绝对的：相对路径不会被悄悄补上 "/"。
        var ex = Assert.Throws<ArgumentException>(() =>
                                                      ResourceLoaderManager.Register("cards/strike.toml", new RecordingLoader()));

        Assert.Contains("absolute", ex.Message);
    }

    [Fact]
    public void Register_Rejects_The_Empty_Path()
    {
        // 空路径曾经表示包根，现在包根只认 UPath.Root。
        Assert.Throws<ArgumentException>(() => ResourceLoaderManager.Register(new UPath(""), new RecordingLoader()));
    }

    [Fact]
    public void FindLoader_Rejects_Relative_Path()
    {
        Assert.Throws<ArgumentException>(() => ResourceLoaderManager.FindLoader("a/b"));
    }

    [Fact]
    public void Path_Escaping_The_Package_Root_Is_Rejected()
    {
        // 越过包根的 ".." 在 UPath 归一化时就被拦下，管理器根本收不到这个值。
        Assert.Throws<ArgumentException>(() => _ = new UPath("/a/../../b"));
    }

    [Fact]
    public void Register_Rejects_Null_Loader()
    {
        Assert.Throws<ArgumentNullException>(() => ResourceLoaderManager.Register("/a", null!));
    }

    [Fact]
    public void Null_Path_Is_Rejected()
    {
        Assert.Throws<ArgumentNullException>(() => ResourceLoaderManager.Register(default(UPath), new RecordingLoader()));
        Assert.Throws<ArgumentNullException>(() => ResourceLoaderManager.FindLoader(default(UPath)));
    }

    [Fact]
    public void Factory_Returns_The_Concrete_Type_And_Base_Interface_Works_In_Collections()
    {
        List<IResourceLoader> loaders = [RecordingLoader.Create()];

        Assert.Single(loaders);
        Assert.IsType<RecordingLoader>(loaders[0]);
    }

    [Fact]
    public void RegisterAttributedLoaders_Registers_Every_Path_To_One_Instance()
    {
        ResourceLoaderManagerAccessor.InvokePrivateStatic("RegisterAttributedLoaders");

        var loader = ResourceLoaderManager.FindLoader("/cards/strike.toml");
        Assert.NotNull(loader);
        Assert.IsType<MultiTargetLoader>(loader);
        Assert.Same(loader, ResourceLoaderManager.FindLoader("/cards"));
        Assert.Same(loader, ResourceLoaderManager.FindLoader("/cards/actions"));
    }

    [Fact]
    public void ValidateLoaderType_Rejects_A_Base_Only_Loader()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => ResourceLoaderManagerAccessor.InvokePrivateStatic("ValidateLoaderType", typeof(BaseOnlyLoader)));

        Assert.Contains("IResourceLoader<BaseOnlyLoader>", ex.Message);
    }

    [Fact]
    public void ValidateLoaderType_Rejects_A_Factory_For_Another_Type()
    {
        var ex = Assert.Throws<InvalidOperationException>(() => ResourceLoaderManagerAccessor.InvokePrivateStatic("ValidateLoaderType", typeof(WrongSelfLoader)));

        Assert.Contains("IResourceLoader<WrongSelfLoader>", ex.Message);
    }

    [Fact]
    public void AutoResourceLoaderAttribute_Allows_Multiple_Registrations()
    {
        var paths = typeof(MultiTargetLoader)
                   .GetCustomAttributes<AutoResourceLoaderAttribute>(inherit: false)
                   .Select(attribute => attribute.Path)
                   .ToList();

        Assert.Equal(2, paths.Count);
        Assert.Contains(new UPath("/cards"), paths);
        Assert.Contains(new UPath("/cards/actions"), paths);
    }

    [Fact]
    public void AutoResourceLoaderAttribute_Normalizes_Its_Path()
    {
        var attribute = new AutoResourceLoaderAttribute(@"\cards//actions\");

        Assert.Equal(new UPath("/cards/actions"), attribute.Path);
    }

    [Fact]
    public void AttributedTypeScanner_Handles_Multiple_Attributes_On_One_Type()
    {
        var types = AttributedTypeScanner.FindTypesWithAttribute<AutoResourceLoaderAttribute>(
            [typeof(ResourceLoaderManagerTests).Assembly]);

        // 一个类型标注多次也只应被发现一次，且 private 嵌套类型也要能看到。
        Assert.Equal(1, types.Count(type => type == typeof(MultiTargetLoader)));
    }

    private sealed class RecordingLoader : IResourceLoader<RecordingLoader>
    {
        public static RecordingLoader Create() => new RecordingLoader();

        public void Load(string packageName, UPath path, Stream stream) { }
    }

    private sealed class BaseOnlyLoader : IResourceLoader
    {
        public void Load(string packageName, UPath path, Stream stream) { }
    }

    private sealed class WrongSelfLoader : IResourceLoader<RecordingLoader>
    {
        public static RecordingLoader Create() => new RecordingLoader();

        public void Load(string packageName, UPath path, Stream stream) { }
    }

    [AutoResourceLoader("/cards")]
    [AutoResourceLoader("/cards/actions")]
    private sealed class MultiTargetLoader : IResourceLoader<MultiTargetLoader>
    {
        public static MultiTargetLoader Create() => new MultiTargetLoader();

        public void Load(string packageName, UPath path, Stream stream) { }
    }
}

public static class ResourceLoaderManagerAccessor
{
    // 自动扫描按要求保持 private；这里用反射守住它，避免为了测试把实现细节提升到 internal。
    public static object? InvokePrivateStatic(string methodName, params object?[]? arguments)
    {
        var method = typeof(ResourceLoaderManager).GetMethod(
                methodName, BindingFlags.Static | BindingFlags.NonPublic)
         ?? throw new MissingMethodException(typeof(ResourceLoaderManager).FullName, methodName);

        try
        {
            return method.Invoke(null, arguments);
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
            throw;
        }
    }

    extension(ResourceLoaderManager)
    {
        public static IResourceLoader? FindLoader(UPath path) => (IResourceLoader?)InvokePrivateStatic("FindLoader", path);
    }
}
