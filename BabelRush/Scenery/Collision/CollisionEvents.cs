using KirisameLib.Event;

namespace BabelRush.Scenery.Collision;

public abstract record ObjectAreaEvent(Area Area, SceneObject Object) : BaseEvent;

public sealed record ObjectEnteredAreaEvent(Area Area, SceneObject Object) : ObjectAreaEvent(Area, Object);

public sealed record ObjectExitedAreaEvent(Area Area, SceneObject Object) : ObjectAreaEvent(Area, Object);

//Area
public abstract record AreaEvent(Area Area) : BaseEvent;

public record AreaTransformedEvent(Area Area) : AreaEvent(Area);