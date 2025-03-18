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
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("mobs", "en", id => (id, ""));
    public static IRegister<MobAnimationSet> MobAnimationSets { get; } = new MobAnimationSetRegister("textures/mobs");
    public static IRegister<MobType> Mobs { get; } =
        CreateSimpleRegister.Data<MobType, MobTypeModel>("mobs", MobType.Default);
}