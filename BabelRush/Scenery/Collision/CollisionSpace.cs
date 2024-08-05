using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Scenery.Collision;

public sealed class CollisionSpace : IDisposable
{
    //Init&Cleanup
    public void Ready()
    {
        EventHandlerSubscriber.InstanceSubscribe(this);
    }

    public void Dispose()
    {
        EventHandlerSubscriber.InstanceUnsubscribe(this);
    }


    //Members
    private List<Area> AreaList { get; } = [];
    private List<SceneObject> ObjectList { get; } = [];
    private List<(Area, SceneObject)> CollidingList { get; } = [];

    public void AddArea(Area area)
    {
        if (InSpace(area)) return;
        AreaList.Add(area);
    }

    public void RemoveArea(Area area)
    {
        AreaList.Remove(area);
    }

    public void AddObject(SceneObject obj)
    {
        if (InSpace(obj)) return;
        ObjectList.Add(obj);
    }

    public void RemoveObject(SceneObject obj)
    {
        ObjectList.Remove(obj);
    }

    public bool InSpace(Area area)
    {
        return AreaList.Contains(area);
    }

    public bool InSpace(SceneObject obj)
    {
        return ObjectList.Contains(obj);
    }


    //EventHandlers
    [EventHandler] [UsedImplicitly]
    private void OnAreaTransformed(AreaTransformedEvent e)
    {
        if (!AreaList.Contains(e.Area)) return;

        foreach (var obj in ObjectList)
        {
            DetectCollision(e.Area, obj);
        }
    }

    [EventHandler] [UsedImplicitly]
    private void OnSceneObjectMoved(SceneObjectMovedEvent e)
    {
        if (!ObjectList.Contains(e.SceneObject)) return;

        foreach (var area in AreaList)
        {
            DetectCollision(area, e.SceneObject);
        }
    }

    private void DetectCollision(Area area, SceneObject obj)
    {
        bool collides = area.Contains(obj);
        bool collided = CollidingList.Contains((area, obj));
        if (!(collides ^ collided)) return;

        if (collides)
        {
            CollidingList.Add((area, obj));
            EventBus.Publish(new ObjectEnteredEvent(area, obj));
        }
        else
        {
            CollidingList.Remove((area, obj));
            EventBus.Publish(new ObjectExitedEvent(area, obj));
        }
    }
}