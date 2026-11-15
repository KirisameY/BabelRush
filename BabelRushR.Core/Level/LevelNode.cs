using System.Collections.Immutable;

namespace BabelRushR.Core.Level;

public sealed class LevelNode
{
    public LevelNode(IRoomDefinition definition, MapPosition position, IEnumerable<LevelNode> nextNodes)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(nextNodes);

        Definition = definition;
        Position = position;
        NextNodes = [.. nextNodes];
    }

    public IRoomDefinition Definition { get; }

    public MapPosition Position { get; }

    public ImmutableArray<LevelNode> NextNodes { get; }
}
