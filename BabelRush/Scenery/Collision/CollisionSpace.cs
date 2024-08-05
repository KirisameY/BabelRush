using System;
using System.Collections.Generic;

using JetBrains.Annotations;

using KirisameLib.Events;
using KirisameLib.Structures;

namespace BabelRush.Scenery.Collision;

public sealed class CollisionSpace : IDisposable
{
    //Init&Cleanup
    public CollisionSpace()
    {
        EventHandlerSubscriber.InstanceSubscribe(this);
    }

    public void Dispose()
    {
        EventHandlerSubscriber.InstanceUnsubscribe(this);
    }


    //Members
    private List<Collider> ColliderList { get; } = [];
    private List<UnorderedPair<Collider>> CollidingPairList { get; } = [];

    public void AddCollider(Collider collider)
    {
        ColliderList.Add(collider);
        EventBus.Publish(new ColliderAddedEvent(collider));
    }

    public void RemoveCollider(Collider collider)
    {
        ColliderList.Remove(collider);
        EventBus.Publish(new ColliderRemovedEvent(collider));
    }

    public bool InSpace(Collider collider)
    {
        return ColliderList.Contains(collider);
    }


    //EventHandlers
    [EventHandler] [UsedImplicitly]
    private void OnColliderMoved(ColliderMovedEvent e)
    {
        if (!ColliderList.Contains(e.Collider))
            return;

        foreach (var collider in ColliderList)
        {
            if (collider == e.Collider) continue;

            UnorderedPair<Collider> pair = new(e.Collider, collider);
            bool collides = collider.CollidesWith(e.Collider);
            bool collided = CollidingPairList.Contains(pair);
            if (!(collides ^ collided)) continue;

            if (collides)
                CollidingPairList.Add(pair);
            else
                CollidingPairList.Remove(pair);

            EventBus.Publish(new CollidedEvent(pair, collides));
        }
    }
}