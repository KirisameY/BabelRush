using BabelRush.Collision;

using KirisameLib.Events;

namespace BabelRush.Scenery;

public abstract class SceneObject
{
    public Scene? Scene { get; set; }
    private double _position;
    public double Position
    {
        get => _position;
        set
        {
            var old = Position;
            _position = value;
            EventBus.Publish(new SceneObjectMovedEvent(this, old, Position));
        }
    }


    //Collision
    private Collider? _collider;
    public Collider? Collider
    {
        get => _collider;
        private set
        {
            _collider?.Dispose();
            _collider = value;
        }
    }

    public void SetCollider(double radius, double offset = 0)
    {
        Collider = new Collider(this, radius, offset);
    }

    public void RemoveCollider()
    {
        Collider = null;
    }
}