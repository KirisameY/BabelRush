using BabelRush.Data;
using BabelRush.Effects;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registering;
using BabelRush.Registering.Registers;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class EffectRegisters
{
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Effects = "effects";
        public const string EffectIcon = $"{SpriteInfoRegisters.Paths.Sprites}/{Effects}";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, EffectScript> EffectScripts { get; } =
        CreateSimpleRegister.Script<EffectScript, EffectScriptModel>(Paths.Effects, EffectScript.Default);

    public static IRegister<RegKey, NameDesc> EffectNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>(Paths.Effects, BabelRush.I18n.DefaultLocal, id => (id, ""));

    public static IRegister<RegKey, SpriteInfo> EffectIcon { get; } =
        SubRegister.Get(SpriteInfoRegisters.Sprites, Paths.Effects);

    public static IRegister<RegKey, EffectType> Effects { get; } =
        CreateSimpleRegister.Data<EffectType, EffectTypeModel>(Paths.Effects, EffectType.Default);
}