using System.Collections.Concurrent;
using System.Reflection;

using BabelRushR.Core.Common;

namespace BabelRushR.Core.Resources;

/// <summary>
///     按包内相对路径把文件分发给对应的 <see cref="IResourceLoader"/>。
/// </summary>
/// <remarks>
///     注册路径既可以是目录也可以是具体文件；查找时按路径分段取最长匹配，所以 <c>a/b/c</c> 的注册
///     优先于 <c>a/b</c>。路径以 <c>/</c> 分隔且大小写敏感；<c>\</c> 会被归一为 <c>/</c>，
///     <c>.</c> 段会被移除，越过包根的 <c>..</c> 会被拒绝。空字符串表示包根。
///     管理器只依赖 <see cref="IResourceLoader"/>；带静态工厂的是派生接口 <c>IResourceLoader&lt;TSelf&gt;</c>。
///     管理器是进程级静态状态：注册应在加载任何包之前完成。
/// </remarks>
public static class ResourceLoaderManager
{
    private static readonly ConcurrentDictionary<string, IResourceLoader> Loaders = new(StringComparer.Ordinal);

    /// <summary>
    ///     把包内相对路径 <paramref name="path"/> 注册给 <paramref name="loader"/>。
    /// </summary>
    /// <param name="path">相对于包根的路径，可以是目录或具体文件；空字符串表示包根。</param>
    /// <param name="loader">负责该路径及其后代路径的加载器。</param>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> 为 <c>null</c>。</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> 越过了包根。</exception>
    /// <exception cref="InvalidOperationException">同一路径已经注册过加载器。</exception>
    public static void Register(string path, IResourceLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        var normalized = NormalizePath(path);
        if (!Loaders.TryAdd(normalized, loader))
        {
            throw new InvalidOperationException(
                $"A resource loader is already registered for package path '{normalized}'.");
        }
    }

    /// <summary>
    ///     查找负责 <paramref name="path"/> 的最具体加载器。
    /// </summary>
    /// <remarks>
    ///     从路径本身开始逐级向上查找，返回第一个命中的注册；因此同时注册了 <c>a/b</c> 与 <c>a/b/c</c> 时，
    ///     <c>a/b/c</c> 及其后代都会命中后者。<paramref name="path"/> 按与 <see cref="Register"/> 相同的规则归一化。
    /// </remarks>
    /// <returns>命中的加载器；没有任何注册覆盖该路径时为 <c>null</c>。</returns>
    public static IResourceLoader? FindLoader(string path)
    {
        var candidate = NormalizePath(path);

        while (true)
        {
            if (Loaders.TryGetValue(candidate, out var loader)) return loader;
            if (candidate.Length == 0) return null;

            var separator = candidate.LastIndexOf('/');
            candidate = separator < 0 ? string.Empty : candidate[..separator];
        }
    }

    /// <summary>仅供测试：清空全部注册。</summary>
    internal static void ClearRegistrations() => Loaders.Clear();

    /// <summary>
    ///     发现标注了 <see cref="AutoResourceLoaderAttribute"/> 的加载器类型，实例化并按特性上的路径注册。
    /// </summary>
    /// <remarks>
    ///     尚未接线：等包的遍历完成后由加载流程调用。同一类型只创建一个实例，供它的所有路径复用；
    ///     本方法不幂等，重复调用会因重复注册而抛异常。
    /// </remarks>
    // ReSharper disable once UnusedMember.Local
    private static void RegisterAttributedLoaders()
    {
        var loaderTypes = AttributedTypeScanner.FindTypesWithAttribute<AutoResourceLoaderAttribute>();
        foreach (var loaderType in loaderTypes)
        {
            ValidateLoaderType(loaderType);
        }

        foreach (var loaderType in loaderTypes)
        {
            var loader = CreateLoader(loaderType);
            foreach (var attribute in loaderType.GetCustomAttributes<AutoResourceLoaderAttribute>(inherit: false))
            {
                Register(attribute.Path, loader);
            }
        }
    }

    // internal 以便测试能直接钉住自动实例化的校验分支；扫描方法本身仍是 private。
    internal static void ValidateLoaderType(Type loaderType)
    {
        AttributedTypeScanner.ValidateRegistrationType(loaderType, typeof(IResourceLoader), "Resource loader");

        // 自动实例化要求实现的是 IResourceLoader<自己>，而不是别的 TSelf。
        // 不能用 MakeGenericType 来检查：约束不满足时它自己就会抛 ArgumentException。
        var implementsSelfFactory = loaderType.GetInterfaces().Any(implemented =>
            implemented.IsGenericType
            && implemented.GetGenericTypeDefinition() == typeof(IResourceLoader<>)
            && implemented.GetGenericArguments()[0] == loaderType);
        if (!implementsSelfFactory)
        {
            throw new InvalidOperationException(
                $"Resource loader '{loaderType.FullName}' must implement IResourceLoader<{loaderType.Name}>.");
        }
    }

    // internal 以便测试能直接验证反射构造；扫描方法本身仍是 private。
    internal static IResourceLoader CreateLoader(Type loaderType)
    {
        try
        {
            var method = typeof(ResourceLoaderManager)
                        .GetMethod(nameof(CreateLoaderCore), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(loaderType);
            return method.Invoke(null, null) as IResourceLoader
                ?? throw new InvalidOperationException(
                    $"Resource loader '{loaderType.FullName}' returned null from IResourceLoader<{loaderType.Name}>.Create().");
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"Failed to create resource loader '{loaderType.FullName}'.", ex.InnerException ?? ex);
        }
    }

    private static TSelf CreateLoaderCore<TSelf>() where TSelf : IResourceLoader<TSelf> => TSelf.Create();

    private static string NormalizePath(string path)
    {
        ArgumentNullException.ThrowIfNull(path);

        var normalized = new List<string>();
        foreach (var segment in path.Replace('\\', '/').Split('/', StringSplitOptions.RemoveEmptyEntries))
        {
            switch (segment)
            {
                case ".":
                    continue;
                case "..":
                    if (normalized.Count == 0)
                    {
                        throw new ArgumentException(
                            $"Package path '{path}' escapes the package root.", nameof(path));
                    }

                    normalized.RemoveAt(normalized.Count - 1);
                    continue;
                default:
                    normalized.Add(segment);
                    break;
            }
        }

        return string.Join('/', normalized);
    }
}
