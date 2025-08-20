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
    public static IRegister<RegKey, NameDesc> EffectNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("effects", "en", id => (id, ""));

    public static IRegister<RegKey, SpriteInfo> EffectIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Sprites, "effects");

    public static IRegister<RegKey, EffectType> Effects { get; } =
        CreateSimpleRegister.Data<EffectType, EffectTypeModel>("cards", EffectType.Default);
}