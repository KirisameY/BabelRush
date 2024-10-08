using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.Parsing;

namespace BabelRush.Mobs;

public record MobTypeDataBox(string Id, string AnimationSet, bool BlocksMovement):IDataBox<MobType, MobTypeDataBox>
{
    public MobType GetAsset()
    {
        var animationSet = Registers.MobRegisters.MobAnimationSets.GetItem(AnimationSet);
        return new(Id, animationSet, BlocksMovement);
    }

    public static MobTypeDataBox FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var animationSet = (string)entry["animation_set"];
        var blocksMovement = (bool)entry["blocks_movement"];

        return new(id, animationSet, blocksMovement);
    }
}