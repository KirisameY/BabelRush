using System.Reflection;

namespace BabelRushR.Core.GamePlay;

// 由于是 Deepseek 写的类代码对人类来说可能有点乱，但是因为算是独立模块所以没关系，保持现状就好

/// <summary>
///     发现并注册标注了 <see cref="GamePlayEventHandlerAttribute"/> 的事件处理器。
/// </summary>
/// <remarks>
///     只处理带特性的类型；带特性但无法注册的类型会直接抛异常，避免静默失败。
/// </remarks>
internal static class GamePlayEventHandlerRegistry
{
    private static readonly Lazy<IReadOnlyList<Type>> DiscoveredTypes =
        new(() => DiscoverHandlerTypes(), LazyThreadSafetyMode.ExecutionAndPublication);


    internal static IDisposable RegisterAll(IGamePlay gamePlay)
        => RegisterAll(gamePlay, DiscoveredTypes.Value);

    internal static IDisposable RegisterAll(IGamePlay gamePlay, IReadOnlyList<Type> handlerTypes)
    {
        ArgumentNullException.ThrowIfNull(gamePlay);
        ArgumentNullException.ThrowIfNull(handlerTypes);

        var registrations = new List<IDisposable>(handlerTypes.Count);
        try
        {
            foreach (var handlerType in handlerTypes.Distinct())
            {
                if (handlerType is null)
                {
                    throw new ArgumentException("Handler type list contains null.", nameof(handlerTypes));
                }

                ValidateHandlerType(handlerType);
                if (RegisterHandler(gamePlay, handlerType) is { } registration)
                {
                    registrations.Add(registration);
                }
            }
        }
        catch
        {
            DisposeAll(registrations);
            throw;
        }

        return new HandlerRegistrations(registrations);
    }


    internal static IReadOnlyList<Type> DiscoverHandlerTypes(IEnumerable<Assembly>? assemblies = null)
    {
        var coreAssembly = typeof(IGamePlayEventHandler).Assembly;

        var handlerTypes = (assemblies ?? CandidateAssemblies(coreAssembly))
                          .SelectMany(LoadableTypes)
                          .Where(type => type.GetCustomAttribute<GamePlayEventHandlerAttribute>(inherit: false) is not null)
                          .Distinct()
                          .ToList();

        // 带特性却无法注册是配置错误，必须在创建游戏时就暴露出来。
        foreach (var handlerType in handlerTypes)
        {
            ValidateHandlerType(handlerType);
        }

        return [.. handlerTypes
            .OrderBy(type => type.Assembly.GetName().Name ?? string.Empty, StringComparer.Ordinal)
            .ThenBy(type => type.FullName ?? type.Name, StringComparer.Ordinal)];
    }


    private static IEnumerable<Assembly> CandidateAssemblies(Assembly coreAssembly)
    {
        var coreName = coreAssembly.GetName().Name;
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            if (assembly.IsDynamic) continue;
            if (assembly == coreAssembly || ReferencesCore(assembly, coreName))
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


    private static void ValidateHandlerType(Type handlerType)
    {
        if (!handlerType.IsClass)
        {
            throw new InvalidOperationException(
                $"GamePlay event handler '{handlerType.FullName}' must be a class.");
        }

        if (handlerType.IsAbstract)
        {
            throw new InvalidOperationException(
                $"GamePlay event handler '{handlerType.FullName}' must not be abstract.");
        }

        if (handlerType.ContainsGenericParameters)
        {
            throw new InvalidOperationException(
                $"GamePlay event handler '{handlerType.FullName}' must not be an open generic type.");
        }

        if (!typeof(IGamePlayEventHandler).IsAssignableFrom(handlerType))
        {
            throw new InvalidOperationException(
                $"GamePlay event handler '{handlerType.FullName}' must implement {nameof(IGamePlayEventHandler)}.");
        }
    }


    private static IDisposable? RegisterHandler(IGamePlay gamePlay, Type handlerType)
    {
        try
        {
            var method = typeof(GamePlayEventHandlerRegistry)
                        .GetMethod(nameof(RegisterHandlerCore), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(handlerType);
            return (IDisposable?)method.Invoke(null, [gamePlay]);
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"Failed to register GamePlay event handler '{handlerType.FullName}'.",
                ex.InnerException ?? ex);
        }
    }

    private static IDisposable? RegisterHandlerCore<THandler>(IGamePlay gamePlay)
        where THandler : IGamePlayEventHandler
        => THandler.Register(gamePlay);


    private static void DisposeAll(List<IDisposable> registrations)
    {
        for (var i = registrations.Count - 1; i >= 0; i--)
        {
            try
            {
                registrations[i].Dispose();
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
                // 保留原始注册异常，清理失败不再覆盖它。
            }
        }
    }


    private sealed class HandlerRegistrations(List<IDisposable> registrations) : IDisposable
    {
        private List<IDisposable>? _registrations = registrations;

        public void Dispose()
        {
            var items = Interlocked.Exchange(ref _registrations, null);
            if (items is null) return;

            List<Exception>? errors = null;
            for (var i = items.Count - 1; i >= 0; i--)
            {
                try
                {
                    items[i].Dispose();
                }
                catch (Exception ex)
                {
                    (errors ??= []).Add(ex);
                }
            }

            if (errors is not null)
            {
                throw new AggregateException(
                    "Failed to dispose one or more GamePlay event handler registrations.", errors);
            }
        }
    }
}
