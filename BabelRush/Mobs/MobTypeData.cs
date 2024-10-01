using System.Collections.Generic;

using BabelRush.Data;

namespace BabelRush.Mobs;

public record MobTypeData(string Id, string AnimationSet, bool BlocksMovement) : IData<MobTypeData>
{
    public MobType ToMobType()
    {
        var animationSet = Registers.MobRegisters.AnimationSets.GetItem(AnimationSet);
        return new(Id, animationSet, BlocksMovement);
    }

    public static MobTypeData FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var animationSet = (string)entry["animation_set"];
        var blocksMovement = (bool)entry["blocks_movement"];

        return new(id, animationSet, blocksMovement);
    }
}