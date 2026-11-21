using System.Collections.Frozen;

namespace BabelRushR.Core.Level;

public sealed class LevelMap(LevelNode startNode)
{
    public LevelNode StartNode { get; } = startNode;

    public FrozenSet<LevelNode> Nodes { get; } = [..EnumerateNodesFrom(startNode)];

    public static LevelMap Empty => field ??= CreateEmpty();

    private static LevelMap CreateEmpty()
    {
        var builder = new LevelMapBuilder(PlaceHolderRoomDefinition.Instance, new MapPosition(0.5, 0.5));
        return builder.Build();
    }

    private static IEnumerable<LevelNode> EnumerateNodesFrom(LevelNode startNode)
    {
        Queue<LevelNode> queue = [];
        queue.Enqueue(startNode);
        while (queue.TryDequeue(out var node))
        {
            yield return node;
            foreach (var next in node.NextNodes)
            {
                if (next.NextNodes.Count > 0) queue.Enqueue(next);
                yield return next;
            }
        }
    }
}