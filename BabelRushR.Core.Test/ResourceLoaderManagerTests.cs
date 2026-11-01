using System.Reflection;

using BabelRushR.Core.Common;
using BabelRushR.Core.Resources;

namespace BabelRushR.Core.Test;

public class ResourceLoaderManagerTests
{
    public ResourceLoaderManagerTests() => ResourceLoaderManager.ClearRegistrations();


    [Fact]
    public void Register_Exact_File_Is_Found_By_Its_Path()
    {
        var loader = new RecordingLoader();

        ResourceLoaderManager.Register("cards/strike.toml", loader);

        Assert.Same(loader, ResourceLoaderManager.FindLoader("cards/strike.toml"));
        Assert.Null(ResourceLoaderManager.FindLoader("cards/defend.toml"));
        Assert.Null(ResourceLoaderManager.FindLoader("cards"));
        Assert.Null(ResourceLoaderManager.FindLoader(""));
    }

    [Fact]
    public void FindLoader_Returns_The_Most_Specific_Registration()
    {
        var directoryLoader = new RecordingLoader();
        var nestedLoader = new RecordingLoader();

        ResourceLoaderManager.Register("a/b", directoryLoader);
        ResourceLoaderManager.Register("a/b/c", nestedLoader);

        // 分段前缀匹配：a/b/c 及其后代命中后者，a/b 的其它后代仍归前者。
        Assert.Same(nestedLoader, ResourceLoaderManager.FindLoader("a/b/c"));
        Assert.Same(nestedLoader, ResourceLoaderManager.FindLoader("a/b/c/d/e.toml"));
        Assert.Same(directoryLoader, ResourceLoaderManager.FindLoader("a/b/d"));
        Assert.Same(directoryLoader, ResourceLoaderManager.FindLoader("a/b"));
        Assert.Null(ResourceLoaderManager.FindLoader("a"));
    }

    [Fact]
    public void Register_And_Find_Normalize_Separators_And_Dot_Segments()
    {
        var loader = new RecordingLoader();

        ResourceLoaderManager.Register(@"\a\b\.\c\", loader);

        Assert.Same(loader, ResourceLoaderManager.FindLoader("a/b/c"));
        Assert.Same(loader, ResourceLoaderManager.FindLoader("a//b/c/"));
        Assert.Same(loader, ResourceLoaderManager.FindLoader("./a/b/c"));
        Assert.Same(loader, ResourceLoaderManager.FindLoader("a/b/x/../c"));
    }

    [Fact]
    public void Root_Registration_Is_The_Fallback()
    {
        var fallbackLoader = new RecordingLoader();
        var cardsLoader = new RecordingLoader();

        ResourceLoaderManager.Register("", fallbackLoader);
        ResourceLoaderManager.Register("cards", cardsLoader);

        Assert.Same(cardsLoader, ResourceLoaderManager.FindLoader("cards/strike.toml"));
        Assert.Same(fallbackLoader, ResourceLoaderManager.FindLoader("audio/bgm.ogg"));
        Assert.Same(fallbackLoader, ResourceLoaderManager.FindLoader("/"));
    }

    [Fact]
    public void FindLoader_Is_Case_Sensitive()
    {
        ResourceLoaderManager.Register("Cards", new RecordingLoader());

        Assert.Null(ResourceLoaderManager.FindLoader("cards"));
    }

    [Fact]
    public void Register_Rejects_Duplicate_Normalized_Path()
    {
        ResourceLoaderManager.Register("a/b", new RecordingLoader());

        var ex = Assert.Throws<InvalidOperationException>(() =>
            ResourceLoaderManager.Register("/a//b/", new RecordingLoader()));

        Assert.Contains("a/b", ex.Message);
    }

    [Fact]
    public void Register_Rejects_Path_Escaping_Root()
    {
        var ex = Assert.Throws<ArgumentException>(() =>
            ResourceLoaderManager.Register("../outside", new RecordingLoader()));

        Assert.Contains("escapes", ex.Message);
    }

    [Fact]
    public void FindLoader_Rejects_Path_Escaping_Root()
    {
        Assert.Throws<ArgumentException>(() => ResourceLoaderManager.FindLoader("a/../../b"));
    }

    [Fact]
    public void Register_Rejects_Null_Loader()
    {
        Assert.Throws<ArgumentNullException>(() => ResourceLoaderManager.Register("a", null!));
    }

    [Fact]
    public void Paths_Reject_Null()
    {
        Assert.Throws<ArgumentNullException>(() => ResourceLoaderManager.Register(null!, new RecordingLoader()));
        Assert.Throws<ArgumentNullException>(() => ResourceLoaderManager.FindLoader(null!));
    }

    [Fact]
    public void Factory_Returns_The_Concrete_Type_And_Base_Interface_Works_In_Collections()
    {
        List<IResourceLoader> loaders = [RecordingLoader.Create()];

        Assert.Single(loaders);
        Assert.IsType<RecordingLoader>(loaders[0]);
    }

    [Fact]
    public void ValidateLoaderType_Accepts_A_Self_Typed_Factory()
    {
        ResourceLoaderManager.ValidateLoaderType(typeof(RecordingLoader));
    }

    [Fact]
    public void CreateLoader_Instantiates_A_Self_Typed_Loader()
    {
        var loader = ResourceLoaderManager.CreateLoader(typeof(RecordingLoader));

        Assert.IsType<RecordingLoader>(loader);
    }

    [Fact]
    public void ValidateLoaderType_Rejects_A_Base_Only_Loader()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => ResourceLoaderManager.ValidateLoaderType(typeof(BaseOnlyLoader)));

        Assert.Contains("IResourceLoader<BaseOnlyLoader>", ex.Message);
    }

    [Fact]
    public void ValidateLoaderType_Rejects_A_Factory_For_Another_Type()
    {
        var ex = Assert.Throws<InvalidOperationException>(
            () => ResourceLoaderManager.ValidateLoaderType(typeof(WrongSelfLoader)));

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
        Assert.Contains("cards", paths);
        Assert.Contains("cards/actions", paths);
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

        public void Load(string packageName, string path, Stream stream) { }
    }

    private sealed class BaseOnlyLoader : IResourceLoader
    {
        public void Load(string packageName, string path, Stream stream) { }
    }

    private sealed class WrongSelfLoader : IResourceLoader<RecordingLoader>
    {
        public static RecordingLoader Create() => new RecordingLoader();

        public void Load(string packageName, string path, Stream stream) { }
    }

    [AutoResourceLoader("cards")]
    [AutoResourceLoader("cards/actions")]
    private sealed class MultiTargetLoader : IResourceLoader<MultiTargetLoader>
    {
        public static MultiTargetLoader Create() => new MultiTargetLoader();

        public void Load(string packageName, string path, Stream stream) { }
    }
}
