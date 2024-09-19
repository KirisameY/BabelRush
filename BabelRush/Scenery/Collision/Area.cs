using System;

using KirisameLib.Core.Events;

namespace BabelRush.Scenery.Collision;

public sealed class Area(double position, double radius)
{
    //Members
    private double _position = position;
    public double Position
    {
        get => _position;
        set
        {
            _position = value;
            EventBus.Publish(new AreaTransformedEvent(this));
        }
    }

    private double _radius = Math.Abs(radius);
    public double Radius
    {
        get => _radius;
        set
        {
            _radius = value;
            EventBus.Publish(new AreaTransformedEvent(this));
        }
    }


    //Methods
    public bool Contains(SceneObject obj) => Math.Abs(Position - obj.Position) <= Radius;
}