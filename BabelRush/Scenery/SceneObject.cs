namespace BabelRush.Scenery;

public abstract class SceneObject
{
    public Scene? Scene { get; set; }
    public double Position
    {
        get;
        set
        {
            var old = Position;
            field = value;
            GameNode.EventBus.Publish(new SceneObjectMovedEvent(this, old, Position));
        }
    }
}