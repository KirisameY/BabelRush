using System.Collections;

namespace BabelRushR.Core.Level;

public class LevelMapBuilder(IRoomDefinition startRoomDefinition, MapPosition startRoomPosition)
{
    public Node StartNode { get; } = new(startRoomDefinition, startRoomPosition);

    public Node CreateNode(IRoomDefinition definition, MapPosition position) => new(definition, position);

    public LevelMap Build()
    {
        Stack<(List<LevelNode> ParentNodes, Node Self, List<LevelNode> Children)> stack = [];
        Dictionary<Node, LevelNode> cache = [];
        List<LevelNode> result = [];
        stack.Push((result, StartNode, new(capacity: StartNode.FollowingNodes.Count)));
        while (stack.TryPeek(out var t))
        {
            var (parentNodes, node, children) = t;

            if (cache.TryGetValue(node, out var baked))
            {
                parentNodes.Add(baked);
                stack.Pop();
            }
            else if (node.FollowingNodes.Count == children.Count)
            {
                baked = new(node.Definition, node.Position, [..children]);
                cache.Add(node, baked);
                parentNodes.Add(baked);
                stack.Pop();
            }
            else
            {
                foreach (var following in node.FollowingNodes)
                {
                    stack.Push((children, following, new(capacity: following.FollowingNodes.Count)));
                }
            }
        }
        return new(result[0]);
    }

    public record Node(IRoomDefinition Definition, MapPosition Position)
    {
        private readonly HashSet<Node> _followingNodes = [];
        public IReadOnlySet<Node> FollowingNodes => field ??= _followingNodes.AsReadOnly();

        public void ConnectTo(Node following)
        {
            if (EnumNodesStartFrom(following).Contains(this)) throw new InvalidOperationException("Circular path is not allowed.");
            _followingNodes.Add(following);
        }

        private IEnumerable<Node> EnumNodesStartFrom(Node node)
        {
            HashSet<Node> enumerated = [];
            Queue<Node> queue = [];
            queue.Enqueue(node);
            while (queue.TryDequeue(out var n))
            {
                yield return n;
                foreach (var following in n.FollowingNodes)
                {
                    if (enumerated.Add(following)) queue.Enqueue(following);
                }
            }
        }
    }
}