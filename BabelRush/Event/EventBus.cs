using System;
using System.Collections.Generic;
using System.Reflection;

namespace BabelRush.Event;

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


    public static void Register<TEvent>(Action<TEvent> handler)
        where TEvent : BaseEvent => AddEventHandler(handler);

    public static void Publish<TEvent>(TEvent @event)
        where TEvent : BaseEvent
    {
        var type = typeof(TEvent);
        do
        {
            var handlerContainerType = typeof(HandlerContainer<>).MakeGenericType(type!);
            var invoke = handlerContainerType.GetMethod(nameof(HandlerContainer<BaseEvent>.InvokeHandler));
            invoke!.Invoke(null, [@event]);
            type = type!.BaseType;
        } while (type != typeof(BaseEvent));
    }
}