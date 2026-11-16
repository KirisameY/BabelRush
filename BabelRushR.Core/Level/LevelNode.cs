using System.Collections.Immutable;
using System.Numerics;

namespace BabelRushR.Core.Level;

public sealed record LevelNode(IRoomDefinition Definition, MapPosition Position, ImmutableList<LevelNode> NextNodes);