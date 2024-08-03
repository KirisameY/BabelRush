using KirisameLib.Events;

namespace BabelRush.Scenery;

public record SceneObjectEvent(SceneObject SceneObject) : BaseEvent;

public record SceneObjectMovedEvent(SceneObject SceneObject, double OldPosition, double NewPosition) : SceneObjectEvent(SceneObject);