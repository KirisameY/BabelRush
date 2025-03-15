using BabelRush.Data;
using BabelRush.Mobs;
using BabelRush.Mobs.Animation;
using BabelRush.Registering;
using BabelRush.Registering.Misc;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class MobRegisters
{
    public static IRegister<NameDesc> MobNameDesc { get; } =
        SimpleRegisterCreate.Lang<NameDesc, NameDescModel>("mobs", "en", id => (id, ""));
    public static IRegister<MobAnimationSet> MobAnimationSets => new MobAnimationSetRegister("textures/mobs");
    public static IRegister<MobType> Mobs { get; } =
        SimpleRegisterCreate.Data<MobType, MobTypeModel>("mobs", MobType.Default);
}