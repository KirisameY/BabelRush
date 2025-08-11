using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Godot;

using KirisameLib.Extensions;
using KirisameLib.Randomization;

namespace BabelRush.Level.Stages.Template.GenerationRules;

public abstract class AfterRule
{
    public abstract void Process(Dictionary<Vector2I, StageNode> nodes, RandomBelt random);

    protected static void ReplaceEffectedNodes(Dictionary<Vector2I, StageNode> nodes, List<(StageNode origin, StageNode @new)> effectedNodes)
    {
        while (effectedNodes.Count > 0)
        {
            var ns = effectedNodes.ToArray();
            effectedNodes.Clear();
            foreach (var (pos, node) in nodes.Where(pair => pair.Value.NextRooms.ContainsAny(ns.Select(t => t.origin))))
            {
                var nextNodes = node.NextRooms as IEnumerable<StageNode>;
                ns.ForEach(t => nextNodes = nextNodes.Select(n => n == t.origin ? t.@new : n));
                var newNode1 = node with { NextRooms = nextNodes.Distinct().ToImmutableArray() };
                effectedNodes.Add((node, newNode1));
                nodes[pos] = newNode1;
            }
        }
    }
}