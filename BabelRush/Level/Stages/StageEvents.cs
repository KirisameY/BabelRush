using System.Collections.Immutable;

using KirisameLib.Event;

namespace BabelRush.Level.Stages;

public abstract record StageEvent(Stage Stage) : BaseEvent;

public sealed record StagePathChosenRequestEvent(Stage Stage, ImmutableArray<StageNode> SelectableNodes) : StageEvent(Stage);