using KirisameLib.Events;
using KirisameLib.Structures;

namespace BabelRush.Collision;

public record CollidedEvent(UnorderedPair<Collider> Pair, bool Collided) : BaseEvent;

//Collider
public record ColliderEvent(Collider Collider) : BaseEvent;

public record ColliderMovedEvent(Collider Collider, double OldPosition, double NewPosition) : ColliderEvent(Collider);

public record ColliderAddedEvent(Collider Collider) : ColliderEvent(Collider);

public record ColliderRemovedEvent(Collider Collider) : ColliderEvent(Collider);