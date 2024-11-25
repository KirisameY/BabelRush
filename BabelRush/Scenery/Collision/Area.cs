using System;

namespace BabelRush.Scenery.Collision;

public sealed class Area(double position, double radius)
{
    //Members
    public double Position
    {
        get;
        set
        {
            field = value;
            Game.EventBus.Publish(new AreaTransformedEvent(this));
        }
    } = position;

    public double Radius
    {
        get;
        set
        {
            field = value;
            Game.EventBus.Publish(new AreaTransformedEvent(this));
        }
    } = Math.Abs(radius);


    //Methods
    public bool Contains(SceneObject obj) => Math.Abs(Position - obj.Position) <= Radius;
}