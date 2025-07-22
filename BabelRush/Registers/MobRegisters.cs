using BabelRush.Data;
using BabelRush.Gui.DisplayInfos.Animation;
using BabelRush.Mobs;
using BabelRush.Registering;
using BabelRush.Registering.Misc;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class MobRegisters
{
    public static IRegister<RegKey, NameDesc> MobNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("mobs", "en", id => (id, ""));
    public static IRegister<RegKey, AnimationSet> MobAnimationSets { get; } = new MobAnimationSetRegister("textures/mobs");
    public static IRegister<RegKey, MobType> Mobs { get; } =
        CreateSimpleRegister.Data<MobType, MobTypeModel>("mobs", MobType.Default);
}