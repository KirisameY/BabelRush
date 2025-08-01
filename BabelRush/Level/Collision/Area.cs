using System;

using BabelRush.Level.Scenery;

namespace BabelRush.Level.Collision;

public sealed class Area(double position, double radius,
                         Action<Area, SceneObject>? objectEntered = null, Action<Area, SceneObject>? objectExited = null)
{
    //Members
    public double Position
    {
        get;
        set
        {
            field = value;
            Game.GameEventBus.Publish(new AreaTransformedEvent(this));
        }
    } = position;

    public double Radius
    {
        get;
        set
        {
            field = value;
            Game.GameEventBus.Publish(new AreaTransformedEvent(this));
        }
    } = Math.Abs(radius);


    public event Action<Area, SceneObject>? ObjectEntered = objectEntered;
    public event Action<Area, SceneObject>? ObjectExited = objectExited;

    internal void RaiseObjectEnteredEvent(SceneObject obj) => ObjectEntered?.Invoke(this, obj);
    internal void RaiseObjectExitedEvent(SceneObject obj) => ObjectExited?.Invoke(this, obj);


    //Methods
    public bool Contains(SceneObject obj) => Math.Abs(Position - obj.Position) <= Radius;
}