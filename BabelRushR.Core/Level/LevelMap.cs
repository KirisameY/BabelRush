using System.Collections.Immutable;

namespace BabelRushR.Core.Level;

public sealed class LevelMap
{
    // todo: 以及，使用主構造器
    public LevelMap(LevelNode startNode, IEnumerable<LevelNode> nodes) // todo: 理論上，所有節點都是從start開始的，那我們只需要從start手動遍歷一遍不就有nodes的集合了嗎？
    {
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
