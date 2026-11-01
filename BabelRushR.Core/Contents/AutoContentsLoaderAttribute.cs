using JetBrains.Annotations;

using Zio;

namespace BabelRushR.Core.Contents;

/// <summary>
///     标记一个 <c>IResourceLoader&lt;TSelf&gt;</c> 实现，并声明它负责的包内路径。
/// </summary>
/// <remarks>
///     同一类型可以标注多次，每个路径都会注册到同一个加载器实例。路径是从包根开始的绝对路径，可以是目录，
///     也可以是具体文件；注册顺序沿用标注顺序，同一路径重复标注不会被去重。
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false), MeansImplicitUse]
public sealed class AutoContentsLoaderAttribute(string path) : Attribute
{
    /// <summary>从包根开始的绝对路径，例如 <c>/cards</c>。</summary>
    /// <remarks>
    ///     特性实参只能是编译期常量，所以构造函数收 <see cref="string"/>，在这里一次性转成 <see cref="UPath"/>；
    ///     归一化交给 <see cref="UPath"/>，<c>\</c> 与 <c>.</c>、<c>..</c> 等写法都会在这里被抹平。
    /// </remarks>
    public UPath Path { get; } = new(path ?? throw new ArgumentNullException(nameof(path)));
}
