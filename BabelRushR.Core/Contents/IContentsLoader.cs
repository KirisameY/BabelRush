using Zio;

namespace BabelRushR.Core.Contents;

/// <summary>
///     包内某一类文件的加载器：按包内路径向 <see cref="ContentsLoaderManager"/> 注册自己负责的范围。
/// </summary>
/// <remarks>
///     加载器实现通常同时实现 <c>IResourceLoader&lt;TSelf&gt;</c>，以便管理器通过静态工厂自动实例化。
///     管理器与集合只依赖本接口，因此不会触发「带静态抽象成员的接口不能作泛型类型实参」的限制。
/// </remarks>
public interface IContentsLoader
{
    /// <summary>
    ///     加载包内的文件。
    /// </summary>
    /// <param name="packageName">文件所属包的命名空间。</param>
    /// <param name="directory">要加载的内容目录。</param>
    void Load(string packageName, DirectoryEntry directory);
}

/// <summary>
///     带静态工厂的加载器接口：<typeparamref name="TSelf"/> 是加载器自身。
/// </summary>
/// <remarks>
///     用 CRTP 形式把工厂的返回类型收紧到具体加载器，同时让管理器只依赖 <see cref="IContentsLoader"/>。
///     实现示例：<c>sealed class CardLoader : IResourceLoader&lt;CardLoader&gt;</c>。
/// </remarks>
/// <typeparam name="TSelf">加载器自身类型。</typeparam>
// ReSharper disable once TypeParameterCanBeVariant
public interface IContentsLoader<TSelf> : IContentsLoader
    where TSelf : IContentsLoader<TSelf>
{
    /// <summary>创建一个加载器实例。</summary>
    /// <remarks>
    ///     无参静态抽象方法让管理器在不知道具体类型的情况下构造加载器；
    ///     实现类型不能是 <c>static class</c>，但可以是 <c>private</c> 嵌套的普通类。
    /// </remarks>
    static abstract TSelf Create();
}