using System.Reflection;

using KirisameLib.Logging;

namespace KirisameLib.Events;

public static class EventHandlerRegisterer
{
    static EventHandlerRegisterer()
    {
        var assembly = Assembly.GetAssembly(typeof(EventHandlerRegisterer));
        if (assembly is not null) RegisterStaticIn(assembly);
    }


    public static void RegisterInstance(object container) => RegisterInstance(container,   true);
    public static void UnRegisterInstance(object container) => RegisterInstance(container, false);

    private static void RegisterInstance(object container, bool register)
    {
        var methodList =
            from method in container.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            where method.CustomAttributes.Any(data => data.AttributeType == typeof(EventHandlerAttribute))
            select method;

        foreach (var method in methodList)
        {
            if (method.GetParameters().Length != 1)
            {
                Logger.Log(LogLevel.Error, nameof(RegisterInstance),
                           $"Method {container.GetType().Name}.{method} has EventHandlerAttribute, but parameters count is not 1");
                continue;
            }

            var eventType = method.GetParameters()[0].ParameterType;
            if (eventType.IsGenericType)
            {
                Logger.Log(LogLevel.Error, nameof(RegisterInstance),
                           $"Method {container.GetType().Name}.{method} has EventHandlerAttribute, but parameters type is in generic");
                continue;
            }

            if (!typeof(BaseEvent).IsAssignableFrom(eventType))
            {
                Logger.Log(LogLevel.Error, nameof(RegisterInstance),
                           $"Method {container.GetType().Name}.{method} has EventHandlerAttribute, but parameters type is not event");
                continue;
            }

            var delegateType = typeof(Action<>).MakeGenericType(eventType);
            var delegateInstance = method.CreateDelegate(delegateType, container);
            if (register)
                typeof(EventBus).GetMethod(nameof(EventBus.Register))!.MakeGenericMethod(eventType).Invoke(null, [delegateInstance]);
            else
                typeof(EventBus).GetMethod(nameof(EventBus.Unregister))!.MakeGenericMethod(eventType).Invoke(null, [delegateInstance]);
        }
    }


    public static void RegisterStaticIn(Assembly assembly)
    {
        var types =
            from type in assembly.GetTypes()
            where type.CustomAttributes.Any(data => data.AttributeType == typeof(EventHandlerAttribute))
            select type;

        foreach (Type type in types)
            RegisterStatic(type);
    }

    public static void RegisterStatic(Type type)
    {
        var methodEventList =
            from method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
            where method.CustomAttributes.Any(data => data.AttributeType == typeof(EventHandlerAttribute))
            select method;

        foreach (MethodInfo method in methodEventList)
        {
            if (method.GetParameters().Length != 1)
            {
                Logger.Log(LogLevel.Error, nameof(RegisterInstance),
                           $"Method {type.Name}.{method} has EventHandlerAttribute, but parameters count is not 1");
                continue;
            }

            var eventType = method.GetParameters()[0].ParameterType;
            if (eventType.IsGenericType)
            {
                Logger.Log(LogLevel.Error, nameof(RegisterInstance),
                           $"Method {type.Name}.{method} has EventHandlerAttribute, but parameters type is in generic");
                continue;
            }

            if (!typeof(BaseEvent).IsAssignableFrom(eventType))
            {
                Logger.Log(LogLevel.Error, nameof(RegisterInstance),
                           $"Method {type.Name}.{method} has EventHandlerAttribute, but parameters type is not event");
                continue;
            }

            var delegateType = typeof(Action<>).MakeGenericType(eventType);
            var delegateInstance = method.CreateDelegate(delegateType);
            typeof(EventBus).GetMethod(nameof(EventBus.Register))!.MakeGenericMethod(eventType).Invoke(null, [delegateInstance]);
        }
    }

    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(EventHandlerRegisterer));
}