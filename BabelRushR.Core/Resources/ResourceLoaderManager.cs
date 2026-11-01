using System.Collections.Concurrent;
using System.Reflection;

using BabelRushR.Core.Common;

using JetBrains.Annotations;

using Zio;

namespace BabelRushR.Core.Resources;

/// <summary>
///     按包内绝对路径把文件分发给对应的 <see cref="IResourceLoader"/>。
/// </summary>
/// <remarks>
///     <para>
///         包内路径统一用 <see cref="UPath"/> 表示：以 <c>/</c> 开头、从包根（<see cref="UPath.Root"/>）算起的绝对路径，
///         例如 <c>/cards/strike.toml</c>。不用裸 <see cref="string"/>，是为了让归一化只存在一处实现——
///         <see cref="UPath"/> 的构造函数会把 <c>\</c> 归一为 <c>/</c>、去掉 <c>.</c> 段、消解 <c>..</c>、
///         合并重复分隔符并去掉结尾分隔符；注册与查找因此不可能各用一套规则而对不上。
///     </para>
///     <para>
///         注册路径既可以是目录也可以是具体文件；查找时按路径分段取最长匹配，所以 <c>/a/b/c</c> 的注册优先于 <c>/a/b</c>。
///         比较按序号进行，因此大小写敏感。<see cref="UPath.Root"/> 就是包根，也是唯一的兜底注册点。
///     </para>
///     <para>
///         注意 <c>..</c> 越过包根的写法（如 <c>/a/../../b</c>）在构造 <see cref="UPath"/> 时就会抛
///         <see cref="ArgumentException"/>，管理器根本收不到这种值；管理器自己只拦「不是绝对路径」。
///     </para>
///     管理器只依赖 <see cref="IResourceLoader"/>；带静态工厂的是派生接口 <c>IResourceLoader&lt;TSelf&gt;</c>。
///     管理器是进程级静态状态：注册应在加载任何包之前完成。
/// </remarks>
public static class ResourceLoaderManager
{
    #region Loader Register

    // 键是 UPath 本身：它的相等性/哈希都基于 FullName 的序号比较，正好是本项目要的大小写敏感语义，
    // 因此不再需要 string 时代那个 StringComparer.Ordinal。
    private static readonly ConcurrentDictionary<UPath, IResourceLoader> Loaders = new();

    /// <summary>
    ///     把包内绝对路径 <paramref name="path"/> 注册给 <paramref name="loader"/>。
    /// </summary>
    /// <param name="path">从包根开始的绝对路径，可以是目录或具体文件；<see cref="UPath.Root"/> 表示包根。</param>
    /// <param name="loader">负责该路径及其后代路径的加载器。</param>
    /// <exception cref="ArgumentNullException"><paramref name="loader"/> 为 <c>null</c>，或 <paramref name="path"/> 是 null 路径。</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> 不是从包根开始的绝对路径。</exception>
    /// <exception cref="InvalidOperationException">同一路径已经注册过加载器。</exception>
    public static void Register(UPath path, IResourceLoader loader)
    {
        ArgumentNullException.ThrowIfNull(loader);

        var normalized = ValidatePath(path);
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
    ///     从路径本身开始按目录逐级向上查找，返回第一个命中的注册；因此同时注册了 <c>/a/b</c> 与 <c>/a/b/c</c> 时，
    ///     <c>/a/b/c</c> 及其后代都会命中后者。
    /// </remarks>
    /// <returns>命中的加载器；没有任何注册覆盖该路径时为 <c>null</c>。</returns>
    /// <exception cref="ArgumentNullException"><paramref name="path"/> 是 null 路径。</exception>
    /// <exception cref="ArgumentException"><paramref name="path"/> 不是从包根开始的绝对路径。</exception>
    private static IResourceLoader? FindLoader(UPath path)
    {
        var candidate = ValidatePath(path);

        while (true)
        {
            if (Loaders.TryGetValue(candidate, out var loader)) return loader;

            // 包根没有上级：GetDirectory("/") 返回的是 null 路径，不在这里收住下一步就会抛。
            if (candidate == UPath.Root) break;

            candidate = candidate.GetDirectory();
        }

        return null;
    }

    // 仅供测试通过反射调用；不提升到 internal，避免测试细节进入程序集契约。
    [UsedImplicitly] private static void ClearRegistrations() => Loaders.Clear();

    #endregion


    #region Private Methods

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

    private static void ValidateLoaderType(Type loaderType)
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

    private static IResourceLoader CreateLoader(Type loaderType)
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

    #endregion
}