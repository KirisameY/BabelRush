using System;
using System.Collections.Generic;

using KirisameLib.Event;

namespace BabelRush.Scenery.Collision;

[EventHandlerContainer]
public sealed partial class CollisionSpace : IDisposable
{
    //Init&Cleanup
    public void Ready()
    {
        SubscribeInstanceHandler(Game.EventBus);
    }

    public void Dispose()
    {
        UnsubscribeInstanceHandler(Game.EventBus);
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
    [EventHandler]
    private void OnAreaTransformed(AreaTransformedEvent e)
    {
        if (!AreaList.Contains(e.Area)) return;

        foreach (var obj in ObjectList)
        {
            DetectCollision(e.Area, obj);
        }
    }

    [EventHandler]
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
            Game.EventBus.Publish(new ObjectEnteredEvent(area, obj));
        }
        else
        {
            CollidingList.Remove((area, obj));
            Game.EventBus.Publish(new ObjectExitedEvent(area, obj));
        }
    }
}