using JetBrains.Annotations;

namespace BabelRushR.Core.Resources;

/// <summary>
///     标记一个 <c>IResourceLoader&lt;TSelf&gt;</c> 实现，并声明它负责的包内路径。
/// </summary>
/// <remarks>
///     同一类型可以标注多次，每个路径都会注册到同一个加载器实例。路径可以是目录，也可以是具体文件；
///     查找时按路径分段做最长前缀匹配，所以 <c>a/b/c</c> 的注册优先于 <c>a/b</c>。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false), MeansImplicitUse]
public sealed class AutoResourceLoaderAttribute(string path) : Attribute
{
    /// <summary>相对于包根的路径，使用 <c>/</c> 分隔。</summary>
    public string Path { get; } = path ?? throw new ArgumentNullException(nameof(path));
}
