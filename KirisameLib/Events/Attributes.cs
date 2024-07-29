namespace KirisameLib.Events;

public abstract class EventHandlerAttribute : Attribute
{
    public abstract Type EventType { get; }
}

[AttributeUsage(AttributeTargets.Method)]
public sealed class EventHandlerAttribute<T> : EventHandlerAttribute
    where T : BaseEvent
{
    public override Type EventType { get; } = typeof(T);
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class StaticEventHandlerContainerAttribute : Attribute;