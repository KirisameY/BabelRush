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
    public void Register_Allows_Duplicate_Paths_And_Keeps_Registration_Order()
    {
        var first = new RecordingLoader();
        var second = new RecordingLoader();

        ResourceLoaderManager.Register("/a/b", first);
        ResourceLoaderManager.Register(new UPath("/a//b/"), second);

        // 同一路径（归一化后相同）允许重复注册：列表按调用顺序追加，两项都保留，后注册的不覆盖先前的。
        var registrations = ResourceLoaderManagerAccessor.Registrations;
        Assert.Equal(2, registrations.Count);
        Assert.Equal(new UPath("/a/b"), registrations[0].Path);
        Assert.Same(first, registrations[0].Loader);
        Assert.Equal(new UPath("/a/b"), registrations[1].Path);
        Assert.Same(second, registrations[1].Loader);
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
    }

    [Fact]
    public void Factory_Returns_The_Concrete_Type_And_Base_Interface_Works_In_Collections()
    {
        List<IResourceLoader> loaders = [RecordingLoader.Create()];

        Assert.Single(loaders);
        Assert.IsType<RecordingLoader>(loaders[0]);
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

        public void Load(string packageName, DirectoryEntry directory) { }
    }

    private sealed class BaseOnlyLoader : IResourceLoader
    {
        public void Load(string packageName, DirectoryEntry directory) { }
    }

    private sealed class WrongSelfLoader : IResourceLoader<RecordingLoader>
    {
        public static RecordingLoader Create() => new RecordingLoader();

        public void Load(string packageName, DirectoryEntry directory) { }
    }

    [AutoResourceLoader("/cards")]
    [AutoResourceLoader("/cards/actions")]
    private sealed class MultiTargetLoader : IResourceLoader<MultiTargetLoader>
    {
        public static MultiTargetLoader Create() => new MultiTargetLoader();

        public void Load(string packageName, DirectoryEntry directory) { }
    }
}

public static class ResourceLoaderManagerAccessor
{
    // 注册表是私有的，这里只读取；不为测试把字段提升到 internal，理由同下面的反射调用。
    public static IReadOnlyList<(UPath Path, IResourceLoader Loader)> Registrations
    {
        get
        {
            var loadersField = typeof(ResourceLoaderManager).GetField("Loaders", BindingFlags.Static | BindingFlags.NonPublic)
             ?? throw new MissingFieldException(typeof(ResourceLoaderManager).FullName, "Loaders");

            return (IReadOnlyList<(UPath Path, IResourceLoader Loader)>)loadersField.GetValue(null)!;
        }
    }

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
}
