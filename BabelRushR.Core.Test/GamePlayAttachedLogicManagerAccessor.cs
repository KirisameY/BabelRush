using System.Reflection;
using System.Runtime.ExceptionServices;

using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.Test;

internal static class GamePlayAttachedLogicManagerAccessor
{
    private static readonly MethodInfo RegisterDiscoveredCoreMethod = typeof(GamePlayAttachedLogicManager)
        .GetMethod("RegisterDiscoveredCore", BindingFlags.Static | BindingFlags.NonPublic)
     ?? throw new MissingMethodException(
            typeof(GamePlayAttachedLogicManager).FullName, "RegisterDiscoveredCore");

    private static readonly MethodInfo DiscoverTypesMethod = typeof(GamePlayAttachedLogicManager)
        .GetMethod("DiscoverTypes", BindingFlags.Static | BindingFlags.NonPublic)
     ?? throw new MissingMethodException(typeof(GamePlayAttachedLogicManager).FullName, "DiscoverTypes");

    // 显式注册与按程序集发现都只对管理器内部开放；测试直接用反射走同一条路径，
    // 不为测试把实现细节提升到 internal/public。
    public static void RegisterDiscoveredCore(IGamePlay gamePlay, IReadOnlyList<Type> handlerTypes)
        => Invoke(RegisterDiscoveredCoreMethod, [gamePlay, handlerTypes]);

    public static IReadOnlyList<Type> DiscoverTypes(IEnumerable<Assembly>? assemblies = null)
        => (IReadOnlyList<Type>)Invoke(DiscoverTypesMethod, [assemblies])!;

    private static object? Invoke(MethodInfo method, object?[] arguments)
    {
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
