using System.Collections.Generic;

namespace BabelRush.Mobs;

public record MobTypeData(string Id, string AnimationSet, bool BlocksMovement)
{
    public MobType ToMobType()
    {
        var animationSet = Registers.MobRegisters.MobAnimationSets.GetItem(AnimationSet);
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