namespace KirisameLib.Events;

public static class EventBus
{
    private static class HandlerContainer<TEvent> where TEvent : BaseEvent
    {
        public static Action<TEvent>? EventHandler { get; set; }
        public static void InvokeHandler(TEvent @event) => EventHandler?.Invoke(@event);
    }

    private static Action<TEvent>? GetEventHandler<TEvent>()
        where TEvent : BaseEvent => HandlerContainer<TEvent>.EventHandler;

    private static void AddEventHandler<TEvent>(Action<TEvent> handler)
        where TEvent : BaseEvent => HandlerContainer<TEvent>.EventHandler += handler;

    private static void RemoveEventHandler<TEvent>(Action<TEvent> handler)
        where TEvent : BaseEvent => HandlerContainer<TEvent>.EventHandler -= handler;


    public static Action Register<TEvent>(Action<TEvent> handler)
        where TEvent : BaseEvent
    {
        AddEventHandler(handler);
        return () => RemoveEventHandler(handler);
    }

    public static void Publish<TEvent>(TEvent @event)
        where TEvent : BaseEvent
    {
        var type = typeof(TEvent);
        for (;;)
        {
            var handlerContainerType = typeof(HandlerContainer<>).MakeGenericType(type!);
            var invoke = handlerContainerType.GetMethod(nameof(HandlerContainer<BaseEvent>.InvokeHandler));
            invoke!.Invoke(null, [@event]);
            if (type == typeof(BaseEvent)) break;
            type = type!.BaseType;
        }
    }
}