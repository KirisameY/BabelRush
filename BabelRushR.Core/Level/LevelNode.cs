using System.Collections.Immutable;

namespace BabelRushR.Core.Level;

public sealed record LevelNode(IRoomDefinition Definition, MapPosition Position, ImmutableList<LevelNode> NextNodes);