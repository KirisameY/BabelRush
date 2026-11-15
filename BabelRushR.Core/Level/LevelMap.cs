using System.Collections.Immutable;

namespace BabelRushR.Core.Level;

public sealed class LevelMap
{
    public LevelMap(LevelNode startNode, IEnumerable<LevelNode> nodes)
    {
        ArgumentNullException.ThrowIfNull(startNode);
        ArgumentNullException.ThrowIfNull(nodes);

        StartNode = startNode;
        Nodes = [.. nodes];

        if (!Nodes.Contains(startNode)) throw new ArgumentException("Start node is not part of the map.", nameof(startNode));
    }

    public LevelNode StartNode { get; }

    public ImmutableArray<LevelNode> Nodes { get; }

    public static LevelMap Empty => field ??= CreateEmpty();

    private static LevelMap CreateEmpty()
    {
        var builder = new LevelMapBuilder();
        var root = builder.Add(PlaceHolderRoomDefinition.Instance, new MapPosition(0.5, 0.5));
        return builder.Build(root);
    }
}
