using System;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Scenery.Collision;

public sealed class Collider : IDisposable
{
    //Initialize&Cleanup
    public Collider(SceneObject sceneObject, double radius, double offset)
    {
        _position = sceneObject.Position + offset;
        Radius = Math.Abs(radius);
        SceneObject = sceneObject;
        EventHandlerSubscriber.InstanceSubscribe(this);
    }
    public void Dispose()
    {
        EventHandlerSubscriber.InstanceUnsubscribe(this);
    }


    //Members
    private double _position;
    public double Position
    {
        get => _position;
        private set
        {
            var old = Position;
            _position = value;
            EventBus.Publish(new ColliderMovedEvent(this, old, value));
        }
    }
    public double Radius { get; }
    public SceneObject SceneObject { get; }


    //Methods
    public bool CollidesWith(Collider other) => Math.Abs(Position - other.Position) < Radius + other.Radius;
    
    
    //EventHandler
    [EventHandler] [UsedImplicitly]
    private void OnSceneObjectMoved(SceneObjectMovedEvent e)
    {
        if (e.SceneObject != SceneObject) return;
        Position = e.SceneObject.Position;          
    }
}