using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace BabelRush.Mobs.Actions;

public class CommonMobActionStrategy(IDictionary<string, List<MobActionTemplate>> actionTable) : MobActionStrategy
{
    private FrozenDictionary<string, (ImmutableList<MobActionTemplate> list, double weightSum)> ActionTable { get; } =
        actionTable.ToFrozenDictionary(pair => pair.Key, pair => (pair.Value.ToImmutableList(), pair.Value.Sum(action => action.Weight)));


    public MobActionTemplate? GetNext(string state)
    {
        if (!ActionTable.TryGetValue(state, out var t)) return null;
        if (t is not (list: { IsEmpty: false } list, var weightSum)) return null;

        var random = Game.Play!.Random;
        var randomValue = random.NextDouble(weightSum);

        foreach (var action in list)
        {
            randomValue -= action.Weight;
            if (randomValue <= 0) return action;
        }

        throw new Exception($"there's something impossible to happen in {nameof(CommonMobActionStrategy)}: "
                          + $"random value seems more then sum of action weights of state {state}, {list}");
    }

    public override MobActionStrategizer NewInstance(Mob mob) => new CommonMobActionStrategizer(this, mob);
}