using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;
using BabelRush.Level.Rooms;
using BabelRush.Registers;

using Godot;

using KirisameLib.Randomization;

namespace BabelRush.Level.Stages.Template.GenerationRules;

public class ReplaceRule(RegKey room, double rate, int ordinalFrom, int ordinalTo, int parallelFrom, int parallelTo) : AfterRule
{
    private RoomTemplate Room => LevelRegisters.Rooms[room];

    public override void Process(Dictionary<Vector2I, StageNode> nodes, RandomBelt random)
    {
        var targets = nodes.Where(pair => pair.Key.Y >= ordinalFrom && pair.Key.Y <= ordinalTo &&
                                      pair.Key.X >= parallelFrom && pair.Key.Y <= parallelTo &&
                                      random.NextDouble() <= rate).ToImmutableArray();
        List<(StageNode origin, StageNode @new)> effectedNodes = [];

        foreach (var (pos, node) in targets)
        {
            var newNode = node with { Room = Room };
            effectedNodes.Add((node, newNode));
            nodes[pos] = newNode;
        }

        ReplaceEffectedNodes(nodes, effectedNodes);
    }
}