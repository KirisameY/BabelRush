using KirisameLib.Event;

namespace BabelRush.Level.Scenery;

public abstract record SceneEvent(Scene Scene) : BaseEvent;

public sealed record SceneReadyEvent(Scene Scene) : SceneEvent(Scene);

public sealed record SceneDisposeEvent(Scene Scene) : SceneEvent(Scene);

