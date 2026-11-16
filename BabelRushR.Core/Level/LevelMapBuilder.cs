using System.Collections.Immutable;

namespace BabelRushR.Core.Level;

public sealed class LevelMapBuilder
{
    private readonly List<LevelNodeBuilder> _nodes = [];
    private bool _built;

    public LevelNodeBuilder Add(IRoomDefinition definition, MapPosition position)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(definition);

        if (!double.IsFinite(definition.Length) || definition.Length < 0)
            throw new ArgumentOutOfRangeException(nameof(definition), "Room length must be a finite non-negative value.");

        if (!IsInsideUnitSquare(position))
            throw new ArgumentOutOfRangeException(nameof(position), "Map position must be inside [0,1]^2.");

        var node = new LevelNodeBuilder(this, definition, position);
        _nodes.Add(node);
        return node;
    }

    public LevelMap Build(LevelNodeBuilder start)
    {
        EnsureMutable();
        ArgumentNullException.ThrowIfNull(start);

        if (!_nodes.Contains(start))
            throw new InvalidOperationException("Start node is not part of this map builder.");

        ValidateReachable(start);
        ValidateAcyclic(start);

        var baked = new Dictionary<LevelNodeBuilder, LevelNode>();
        var nodes = new List<LevelNode>(_nodes.Count);
        foreach (var node in _nodes) nodes.Add(Bake(node, baked));

        _built = true;
        return new LevelMap(baked[start], nodes);
    }

    internal void EnsureMutable()
    {
        if (_built) throw new InvalidOperationException("The map has already been built.");
    }

    private static bool IsInsideUnitSquare(MapPosition position) =>
        double.IsFinite(position.X) && double.IsFinite(position.Y)
     && position.X is >= 0 and <= 1
     && position.Y is >= 0 and <= 1;

    private void ValidateReachable(LevelNodeBuilder start)
    {
        var visited = new HashSet<LevelNodeBuilder>();
        var pending = new Stack<LevelNodeBuilder>();
        pending.Push(start);

        while (pending.TryPop(out var node))
        {
            if (!visited.Add(node)) continue;
            foreach (var next in node.Next) pending.Push(next);
        }

        if (visited.Count != _nodes.Count)
            throw new InvalidOperationException("All map nodes must be reachable from the start node.");
    }

    private static void ValidateAcyclic(LevelNodeBuilder start)
    {
        var state = new Dictionary<LevelNodeBuilder, bool>();

        void Visit(LevelNodeBuilder node)
        {
            if (state.TryGetValue(node, out var finished))
            {
                if (!finished) throw new InvalidOperationException("The map must be acyclic.");
                return;
            }

            state[node] = false;
            foreach (var next in node.Next) Visit(next);
            state[node] = true;
        }

        Visit(start);
    }

    private static LevelNode Bake(LevelNodeBuilder node, Dictionary<LevelNodeBuilder, LevelNode> baked)
    {
        if (baked.TryGetValue(node, out var result)) return result;

        // 先烘焙后继再构造当前节点；菱形结构靠记忆化复用同一份实例。
        var next = node.Next.Select(nextNode => Bake(nextNode, baked)).ToImmutableList();
        result      = new LevelNode(node.Definition, node.Position, next);
        baked[node] = result;
        return result;
    }
}