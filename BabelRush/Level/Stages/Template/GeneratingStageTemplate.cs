using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Data;
using BabelRush.Level.Rooms;
using BabelRush.Level.Stages.Template.GenerationRules;
using BabelRush.Registers;
using BabelRush.Utils;

using Godot;

using KirisameLib.Extensions;
using KirisameLib.Randomization;

namespace BabelRush.Level.Stages.Template;

/// <param name="paths"> Amount of path in generated map, if less than <c>parallels</c>, limit to it. </param>
public class GeneratingStageTemplate(RegKey id, RegKey startRoomId, int length, int parallels, int paths,
                                     ImmutableArray<(RegKey roomId, double weight)> rooms,
                                     ImmutableArray<SeparationRule> separationRules,
                                     ImmutableArray<AfterRule> afterRules) : StageTemplate(id)
{
    private RoomTemplate StartRoom => LevelRegisters.Rooms[startRoomId];

    [field: AllowNull, MaybeNull]
    private ImmutableList<(RoomTemplate room, double weight)> Rooms =>
        field ??= rooms.Select(t => (LevelRegisters.Rooms[t.roomId], t.weight)).ToImmutableList();

    private readonly double _weightSum = rooms.Sum(r => r.weight);

    // generate paths
    public override Stage GenerateStage(RandomBelt random)
    {
        Dictionary<Vector2I, List<Vector2I>> pathDict = new(); // x for parallel, y for forward
        foreach (int path in random.Shuffle(Enumerable.Range(0, parallels).Take(paths)))
        {
            pathDict.TryAdd(new(path, 1), []);
            int prev = path;
            for (int i = 2; i <= length; i++)
            {
                // ReSharper disable AccessToModifiedClosure
                var separations = separationRules.Where(s => s.From < i && s.To >= i).Select(s => s.SeparateBefore).ToArray();
                // ReSharper restore AccessToModifiedClosure
                int x;
                do
                {
                    x = prev + random.NextInt(-1, 2);
                } while (separations.Any(s => (prev < s && x >= s) || (prev >= s && x < s)) || x < 0 || x >= parallels);

                pathDict[new(prev, i - 1)].Add(new(x, i));
                pathDict.TryAdd(new(x, i), []);
                prev = x;
            }
        }

        Dictionary<Vector2I, StageNode> nodeDict = new();
        foreach (var (pos, rears) in pathDict.OrderByDescending(pair => pair.Key.Y))
        {
            var room = random.RandomItemWithWeight(Rooms, _weightSum);
            nodeDict.Add(pos, new(room, rears.Distinct().Select(v => nodeDict[v]).ToImmutableArray(), pos.Y,
                                  pos / new Vector2(parallels / 2f, length) + new Vector2(0.5f, 0)));
        }
        afterRules.ForEach(r => r.Process(nodeDict, random));


        var startNode = new StageNode(StartRoom, nodeDict.Where(pair => pair.Key.Y == 1).Select(p => p.Value).ToImmutableArray(),
                                      0, new(0.5f, 0));
        return new(this, startNode);
    }
}