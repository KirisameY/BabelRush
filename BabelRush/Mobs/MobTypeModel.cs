using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.Model;

namespace BabelRush.Mobs;

public record MobTypeModel(string Id, string AnimationSet, bool BlocksMovement) : IDataModel<MobType>
{
    public MobType Convert()
    {
        var animationSet = Registers.MobRegisters.MobAnimationSets.GetItem(AnimationSet);
        return new(Id, animationSet, BlocksMovement);
    }

    public static MobTypeModel FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        var animationSet = (string)entry["animation_set"];
        var blocksMovement = (bool)entry["blocks_movement"];

        return new(id, animationSet, blocksMovement);
    }
    
    public static IModel<MobType>[] FromSource(byte[] source, out ModelParseErrorInfo errorMessages)
    {
        throw new System.NotImplementedException();
    }
}