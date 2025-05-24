using System;
using System.Collections.Generic;

using KirisameLib.Event;
using KirisameLib.Extensions;

namespace BabelRush.Scenery.Collision;

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

    private void RemoveCollision(Area area) => CollidingList.RemoveWhere(t => t.Area == area);

    private void RemoveCollision(SceneObject obj) => CollidingList.RemoveWhere(t => t.Obj == obj);

    // private void RemoveCollision(Area area, SceneObject obj) => CollidingList.Remove((area, obj));

    private void DetectCollision(Area area) => ObjectList.ForEach(obj => DetectCollision(area, obj));

    private void DetectCollision(SceneObject obj) => AreaList.ForEach(area => DetectCollision(area, obj));

    private void DetectCollision(Area area, SceneObject obj)
    {
        bool collides = area.Contains(obj);
        bool collided = CollidingList.Contains((area, obj));
        if (!(collides ^ collided)) return;

        if (collides)
        {
            CollidingList.Add((area, obj));
            Game.GameEventBus.Publish(new ObjectEnteredAreaEvent(area, obj));
        }
        else
        {
            CollidingList.Remove((area, obj));
            Game.GameEventBus.Publish(new ObjectExitedAreaEvent(area, obj));
        }
    }

    #endregion


    #region EventHandlers

    [EventHandler]
    private void OnAreaTransformed(AreaTransformedEvent e) // 这里可能会导致事件发生时和处理时状态不一致，但现在先不修
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