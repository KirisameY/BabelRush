using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

using BabelRushR.Core.Common;

using KirisameY.Relinq.Extensions;

namespace BabelRushR.Core.GamePlay;

// 由于是 Vibe 的类代码对人类来说可能有点乱，但是因为算是独立模块所以没关系，不涉及相关逻辑时保持现状就好

/// <summary>
///     发现并注册标注了 <see cref="AutoGamePlayEventHandlerAttribute"/> 的附加逻辑类型。
/// </summary>
public static class GamePlayAttachedLogicManager
{
    private static readonly Lazy<IReadOnlyList<Type>> DiscoveredTypes =
        new(() => DiscoverTypes(), LazyThreadSafetyMode.ExecutionAndPublication);

    private static readonly ConditionalWeakTable<IGamePlay, object> _initialized = [];

    public static void RegisterDiscovered(IGamePlay gamePlay)
    {
        var method = new StackFrame(1).GetMethod();
        if (method?.DeclaringType?.IsAssignableTo(typeof(IGamePlay)) is not true)
        {
            throw new InvalidOperationException("This method can only called by GamePlay class.");
        }
        if (!_initialized.TryAdd(gamePlay, new()))
        {
            throw new InvalidOperationException("The GamePlay instance has already initialized.");
        }

        RegisterDiscoveredCore(gamePlay, DiscoveredTypes.Value);
    }

    private static void RegisterDiscoveredCore(IGamePlay gamePlay, IReadOnlyList<Type> handlerTypes)
    {
        foreach (var handlerType in handlerTypes.Distinct())
        {
            if (gamePlay.Disposed) return; // 算是个冗余检查
            ValidateType(handlerType);
            if (RegisterType(gamePlay, handlerType) is IDisposable d) gamePlay.Disposing += d.Dispose;
        }
    }


    private static IReadOnlyList<Type> DiscoverTypes(IEnumerable<Assembly>? assemblies = null)
    {
        var types = AttributedTypeScanner.FindTypesWithAttribute<AutoGamePlayEventHandlerAttribute>(assemblies);
        types.ForEach(ValidateType);
        return types;
    }


    private static void ValidateType(Type handlerType)
        => AttributedTypeScanner.ValidateRegistrationType(
            handlerType, typeof(IGamePlayAttachedLogic), "GamePlay attached logic");


    private static object? RegisterType(IGamePlay gamePlay, Type type)
    {
        try
        {
            var method = typeof(GamePlayAttachedLogicManager)
                        .GetMethod(nameof(RegisterCore), BindingFlags.Static | BindingFlags.NonPublic)!
                        .MakeGenericMethod(type);
            return method.Invoke(null, [gamePlay]);
        }
        catch (TargetInvocationException ex)
        {
            throw new InvalidOperationException(
                $"Failed to register GamePlay attached logic '{type.FullName}'.",
                ex.InnerException ?? ex);
        }
    }

    private static IDisposable? RegisterCore<T>(IGamePlay gamePlay) where T : IGamePlayAttachedLogic => T.Register(gamePlay);
}