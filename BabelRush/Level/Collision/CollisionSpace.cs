using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Level.Scenery;

using KirisameLib.Event;
using KirisameLib.Extensions;

namespace BabelRush.Level.Collision;

[EventHandlerContainer]
public sealed partial class CollisionSpace : IDisposable
{
    #region Init&Cleanup

    public void Ready()
    {
        SubscribeInstanceHandler(Game.GameEventBus);
    }

    public void Dispose()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
    }

    #endregion


    #region Members

    private HashSet<Area> AreaList { get; } = [];
    private HashSet<SceneObject> ObjectList { get; } = [];
    private HashSet<(Area Area, SceneObject Obj)> CollidingList { get; } = [];

    public void AddArea(Area area)
    {
        if (!AreaList.Add(area)) return;
        DetectCollision(area);
    }

    public void RemoveArea(Area area)
    {
        if (!AreaList.Contains(area)) return;
        RemoveCollision(area);
        AreaList.Remove(area);
    }

    public void AddObject(SceneObject obj)
    {
        if (!ObjectList.Add(obj)) return;
        DetectCollision(obj);
    }

    public void RemoveObject(SceneObject obj)
    {
        if (!ObjectList.Contains(obj)) return;
        RemoveCollision(obj);
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

    #endregion


    #region Detect

    private void RemoveCollision(Area area) => CollidingList.Where(t => t.Area == area).ToArray().ForEach(t =>
    {
        ObjectExitArea(t.Area, t.Obj);
    });

    private void RemoveCollision(SceneObject obj) => CollidingList.Where(t => t.Obj == obj).ToArray().ForEach(t =>
    {
        ObjectExitArea(t.Area, t.Obj);
    });

    // private void RemoveCollision(Area area, SceneObject obj) => CollidingList.Remove((area, obj));

    private void DetectCollision(Area area) => ObjectList.ForEach(obj => DetectCollision(area, obj));

    private void DetectCollision(SceneObject obj) => AreaList.ForEach(area => DetectCollision(area, obj));

    private void DetectCollision(Area area, SceneObject obj)
    {
        bool collides = area.Contains(obj);
        bool collided = CollidingList.Contains((area, obj));
        if (!(collides ^ collided)) return;

        if (collides) ObjectEnterArea(area, obj);
        else ObjectExitArea(area, obj);
    }

    private void ObjectEnterArea(Area area, SceneObject obj)
    {
        CollidingList.Add((area, obj));
        area.RaiseObjectEnteredEvent(obj);
        Game.GameEventBus.Publish(new ObjectEnteredAreaEvent(area, obj));
    }

    private void ObjectExitArea(Area area, SceneObject obj)
    {
        CollidingList.Remove((area, obj));
        area.RaiseObjectExitedEvent(obj);
        Game.GameEventBus.Publish(new ObjectExitedAreaEvent(area, obj));
    }

    #endregion


    #region EventHandlers

    [EventHandler]
    private void OnAreaTransformed(AreaTransformedEvent e)
    {
        if (!AreaList.Contains(e.Area)) return;

        DetectCollision(e.Area);
    }

    [EventHandler]
    private void OnSceneObjectMoved(SceneObjectMovedEvent e)
    {
        if (!ObjectList.Contains(e.SceneObject)) return;

        DetectCollision(e.SceneObject);
    }

    #endregion
}