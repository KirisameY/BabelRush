using KirisameLib.Event;

namespace BabelRush.Scenery;

public abstract record SceneEvents(Scene Scene) : BaseEvent;

public sealed record SceneReadyEvent(Scene Scene) : SceneEvents(Scene);

public sealed record SceneDisposeEvent(Scene Scene) : SceneEvents(Scene);

