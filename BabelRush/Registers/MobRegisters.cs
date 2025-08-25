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
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Mobs = "mobs";
        public const string MobAnimationSet = $"{SpriteInfoRegisters.Paths.AnimationSets}/{Mobs}";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, NameDesc> MobNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>(Paths.Mobs, BabelRush.I18n.DefaultLocal, id => (id, ""));
    public static IRegister<RegKey, AnimationSet> MobAnimationSet { get; } =
        SubRegister.Get(SpriteInfoRegisters.AnimationSets, Paths.Mobs);
    public static IRegister<RegKey, MobType> Mobs { get; } =
        CreateSimpleRegister.Data<MobType, MobTypeModel>(Paths.Mobs, MobType.Default);
}