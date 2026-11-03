using System.Reflection;

namespace BabelRushR.Core.Common;

/// <summary>
///     在 Core 及其已加载的引用程序集里发现带指定特性的类型，并提供自动注册前的类型校验。
/// </summary>
/// <remarks>
///     GamePlay 事件处理器与资源加载器共用这套规则：只扫描 Core 程序集和直接引用 Core 的非动态程序集；
///     <c>GetTypes()</c> 包含 private 嵌套类型；<see cref="ReflectionTypeLoadException"/> 时保留成功加载的类型。
///     结果按「程序集名 → 类型全名」排序，保证注册顺序稳定。
/// </remarks>
internal static class AttributedTypeScanner
{
    private static readonly Assembly CoreAssembly = typeof(AttributedTypeScanner).Assembly;

    internal static IReadOnlyList<Type> FindTypesWithAttribute<TAttribute>(IEnumerable<Assembly>? assemblies = null)
        where TAttribute : Attribute
    {
        return [.. (assemblies ?? CandidateAssemblies())
            .SelectMany(LoadableTypes)
            // 用 IsDefined 而不是 GetCustomAttribute：特性可能 AllowMultiple，后者遇到多个会抛异常。
            .Where(type => type.IsDefined(typeof(TAttribute), inherit: false))
            .Distinct()
            .OrderBy(type => type.Assembly.GetName().Name ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(type => type.FullName ?? type.Name, StringComparer.Ordinal)];
    }

    /// <summary>
    ///     校验类型可以作为自动注册的实现：具体类、非开放泛型、实现了指定接口。
    /// </summary>
    /// <param name="type">待校验的类型。</param>
    /// <param name="requiredInterface">该角色必须实现的接口，用于报错信息与可赋值性检查。</param>
    /// <param name="role">报错信息里的角色名，例如 <c>"Resource loader"</c>。</param>
    internal static void ValidateRegistrationType(Type type, Type requiredInterface, string role)
    {
        if (!type.IsClass)
        {
            throw new InvalidOperationException($"{role} '{type.FullName}' must be a class.");
        }

        if (type.IsAbstract)
        {
            throw new InvalidOperationException($"{role} '{type.FullName}' must not be abstract.");
        }

        if (type.ContainsGenericParameters)
        {
            throw new InvalidOperationException($"{role} '{type.FullName}' must not be an open generic type.");
        }

        if (!requiredInterface.IsAssignableFrom(type))
        {
            throw new InvalidOperationException($"{role} '{type.FullName}' must implement {requiredInterface.Name}.");
        }
    }


    private static IEnumerable<Assembly> CandidateAssemblies()
    {
        var coreName = CoreAssembly.GetName().Name;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic) continue;
            if (assembly == CoreAssembly || ReferencesCore(assembly, coreName))
            {
                yield return assembly;
            }
        }
    }

    private static bool ReferencesCore(Assembly assembly, string? coreName)
    {
        if (coreName is null) return false;

        try
        {
            return assembly.GetReferencedAssemblies().Any(reference => reference.Name == coreName);
        }
        catch (Exception ex) when (ex is FileNotFoundException or FileLoadException
                                       or BadImageFormatException or NotSupportedException)
        {
            return false;
        }
    }

    private static IEnumerable<Type> LoadableTypes(Assembly assembly)
    {
        try
        {
            return assembly.GetTypes();
        }
        catch (ReflectionTypeLoadException ex)
        {
            return ex.Types.OfType<Type>();
        }
        catch (NotSupportedException)
        {
            return [];
        }
    }
}
