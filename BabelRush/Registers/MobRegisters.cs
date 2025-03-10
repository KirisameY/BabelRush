using System;

using BabelRush.Data;
using BabelRush.Mobs;
using BabelRush.Mobs.Animation;
using BabelRush.Registering;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class MobRegisters
{
    public static IRegister<NameDesc> MobNameDesc { get; } =
        SimpleRegisterCreate.Lang<NameDesc, NameDescModel>("mobs", "en", id => (id, ""));
    public static IRegister<MobAnimationSet> MobAnimationSets => throw new NotImplementedException();
    public static IRegister<MobType> Mobs { get; } =
        SimpleRegisterCreate.Data<MobType, MobTypeModel>("mobs", MobType.Default);

    // todo:
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