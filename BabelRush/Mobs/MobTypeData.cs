using BabelRush.Data;

using Tomlyn.Model;

namespace BabelRush.Mobs;

public record MobTypeData(string Id, string AnimationSet, bool BlocksMovement) : ITomlData<MobTypeData>
{
    public MobType ToMobType()
    {
        var animationSet = Registers.MobRegisters.AnimationSets.GetItem(AnimationSet);
        return new(Id, animationSet, BlocksMovement);
    }

    public static MobTypeData FromTomlEntry(TomlTable entry)
    {
        var id = (string)entry["id"];
        var animationSet = (string)entry["animation_set"];
        var blocksMovement = (bool)entry["blocks_movement"];

        return new(id, animationSet, blocksMovement);
    }
}