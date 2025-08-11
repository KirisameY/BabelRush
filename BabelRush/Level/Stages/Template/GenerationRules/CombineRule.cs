using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;
using BabelRush.Level.Rooms;
using BabelRush.Registers;

using Godot;

using KirisameLib.Randomization;

namespace BabelRush.Level.Stages.Template.GenerationRules;

public class CombineRule(int ordinal, int from, int to, int newPos, Vector2 newDisplayPos, RegKey? room) : AfterRule
{
    private RoomTemplate? Room => room is not null ? LevelRegisters.Rooms[room] : null;

    public override void Process(Dictionary<Vector2I, StageNode> nodes, RandomBelt random)
    {
        var targets = nodes.Where(pair => pair.Key.Y == ordinal && pair.Key.X >= from && pair.Key.X <= to)
                           .ToImmutableArray();
        if (targets.Length == 0) return;

        var newRoom = Room ?? random.Draw(targets).Value.Room;
        var newNode = new StageNode(newRoom, targets.SelectMany(pair => pair.Value.NextRooms).Distinct().ToImmutableArray(),
                                    ordinal, newDisplayPos);

        List<(StageNode origin, StageNode @new)> effectedNodes = [];
        foreach (var (pos, node) in targets)
        {
            effectedNodes.Add((node, newNode));
            nodes.Remove(pos);
        }
        var newX = Math.Clamp(newPos, from, to);
        nodes.Add(new(newX, ordinal), newNode);

        ReplaceEffectedNodes(nodes, effectedNodes);
    }
}