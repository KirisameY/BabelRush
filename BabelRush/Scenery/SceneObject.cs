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
            Game.EventBus.Publish(new SceneObjectMovedEvent(this, old, Position));
        }
    }
    public virtual bool Collidable => false;

    protected virtual void _EnterScene() { }
    protected virtual void _ExitScene() { }

    internal void EnterScene()
    {
        _EnterScene();
    }

    internal void ExitScene()
    {
        _ExitScene();
    }
}