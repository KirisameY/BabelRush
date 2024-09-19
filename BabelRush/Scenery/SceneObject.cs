using Godot;

using KirisameLib.Core.Events;

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

    public abstract Node CreateInterface();
}