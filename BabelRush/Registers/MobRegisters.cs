using BabelRush.Data;
using BabelRush.Mobs;
using BabelRush.Mobs.Animation;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

public static partial class MobRegisters
{
    public static IRegister<NameDesc> MobNameDesc { get; }
    public static IRegister<MobAnimationSet> MobAnimationSets { get; }
    public static IRegister<MobType> Mobs { get; }


    // [LangRegister<NameDescModel, NameDesc>("mobs")]
    // private static readonly LocalizedRegister<NameDesc> MobNameDescRegister =
    //     new("en", id => (id, ""));
    //
    // [DataRegister<MobTypeModel, MobType>("mobs")]
    // private static readonly CommonRegister<MobType> MobsRegister =
    //     new(_ => MobType.Default);
    //
    // // Mob animation data
    // private static readonly MobAnimationSetRegister MobAnimationSetRegister = new(MobAnimationSet.Default);
    // public static IRegister<MobAnimationSet> MobAnimationSets => MobAnimationSetRegister;
    //
    // static partial void _Register()
    // {
    //     FileLoader.AddDefaultResRegistrant("textures/mobs", new(new MobAnimationDefaultRegistrant(MobAnimationSetRegister)));
    //     FileLoader.AddLocalResRegistrant("textures/mobs", new(new MobAnimationLocalizedRegistrant(MobAnimationSetRegister)));
    // }
}