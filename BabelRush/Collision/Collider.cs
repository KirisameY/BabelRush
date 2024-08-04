using System;

using KirisameLib.Events;

namespace BabelRush.Collision;

public class Collider(double position, double radius)
{
    //Getters
    public static Collider FromPosition(double position, double radius) => new(position, radius);

    public static Collider FromArea(double start, double end) => new((start + end) / 2, (end - start) / 2);


    //Members
    private double _position = position;
    public double Position
    {
        get => _position;
        set
        {
            var old = Position;
            _position = value;
            EventBus.Publish(new ColliderMovedEvent(this, old, value));
        }
    }
    public double Radius { get; } = Math.Abs(radius);


    //Methods
    public bool CollidesWith(Collider other) => Math.Abs(Position - other.Position) < Radius + other.Radius;
}