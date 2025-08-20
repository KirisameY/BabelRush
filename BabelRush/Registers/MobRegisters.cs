using BabelRush.Data;
using BabelRush.Gui.DisplayInfos.Animation;
using BabelRush.Mobs;
using BabelRush.Registering;
using BabelRush.Registering.Registers;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class MobRegisters
{
    public static IRegister<RegKey, NameDesc> MobNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("mobs", BabelRush.I18n.DefaultLocal, id => (id, ""));
    public static IRegister<RegKey, AnimationSet> MobAnimationSets { get; } =
        SubRegister.Create(SpriteInfoRegisters.AnimationSets, "mobs");
    public static IRegister<RegKey, MobType> Mobs { get; } =
        CreateSimpleRegister.Data<MobType, MobTypeModel>("mobs", MobType.Default);
}