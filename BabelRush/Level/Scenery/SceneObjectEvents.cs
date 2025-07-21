using KirisameLib.Event;

namespace BabelRush.Level.Scenery;

public abstract record SceneObjectEvent(SceneObject SceneObject) : BaseEvent;

public sealed record SceneObjectMovedEvent(SceneObject SceneObject, double OldPosition, double NewPosition) : SceneObjectEvent(SceneObject);