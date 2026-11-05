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
    public UPath Path => new(path);
}