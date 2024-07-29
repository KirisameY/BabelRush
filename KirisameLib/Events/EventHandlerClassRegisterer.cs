using System.Reflection;

using KirisameLib.Logging;

namespace KirisameLib.Events;

public static class EventHandlerClassRegisterer
{
    static EventHandlerClassRegisterer()
    {
        var assembly = Assembly.GetAssembly(typeof(EventHandlerClassRegisterer));
        if (assembly is not null) RegisterStaticEventHandlersIn(assembly);
    }


    public static void RegisterInstanceEventHandlerContainer(object container)
    {
        var methodEventList =
            from method in container.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            where method.CustomAttributes.Any(data => data.AttributeType.IsSubclassOf(typeof(EventHandlerAttribute)))
            select (method, method.GetCustomAttribute<EventHandlerAttribute>()!.EventType);

        foreach ((MethodInfo method, Type eventType) in methodEventList)
        {
            if (method.GetParameters().Length != 1 || method.GetParameters()[0].ParameterType != eventType)
            {
                Logger.Log(LogLevel.Error, nameof(RegisterInstanceEventHandlerContainer),
                           $"Method {method} has EventHandlerAttribute<{eventType.Name}>, but signature is not ({eventType.Name})");
                continue;
            }

            var delegateType = typeof(Action<>).MakeGenericType(eventType);
            var delegateInstance = method.CreateDelegate(delegateType, container);
            typeof(EventBus).GetMethod(nameof(EventBus.Register))!.MakeGenericMethod(eventType).Invoke(null, [delegateInstance]);
        }
    }

    public static void RegisterStaticEventHandlersIn(Assembly assembly)
    {
        var types =
            from type in assembly.GetTypes()
            where type.CustomAttributes.Any(data => data.AttributeType == typeof(StaticEventHandlerContainerAttribute))
            select type;

        foreach (Type type in types)
            RegisterStaticEventHandlerContainer(type);
    }

    private static void RegisterStaticEventHandlerContainer(Type type)
    {
        var methodEventList =
            from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            where method.CustomAttributes.Any(data => data.AttributeType.IsSubclassOf(typeof(EventHandlerAttribute)))
            select (method, method.GetCustomAttribute<EventHandlerAttribute>()!.EventType);

        foreach ((MethodInfo method, Type eventType) in methodEventList)
        {
            if (method.GetParameters().Length != 1 || method.GetParameters()[0].ParameterType != eventType)
            {
                Logger.Log(LogLevel.Error, nameof(RegisterStaticEventHandlerContainer),
                           $"Method {method} has EventHandlerAttribute<{eventType.Name}>, but signature is not ({eventType.Name})");
                continue;
            }

            var delegateType = typeof(Action<>).MakeGenericType(eventType);
            var delegateInstance = method.CreateDelegate(delegateType);
            typeof(EventBus).GetMethod(nameof(EventBus.Register))!.MakeGenericMethod(eventType).Invoke(null, [delegateInstance]);
        }
    }

    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(EventHandlerClassRegisterer));
}