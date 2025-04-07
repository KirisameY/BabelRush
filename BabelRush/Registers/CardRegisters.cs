using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registering;
using BabelRush.Registering.Registers;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class CardRegisters
{
    public static IRegister<RegKey, NameDesc> CardNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("cards", "en", id => (id, ""));

    public static IRegister<RegKey, SpriteInfo> CardIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Sprites, "cards");

    public static IRegister<RegKey, CardType> Cards { get; } =
        CreateSimpleRegister.Data<CardType, CardTypeModel>("cards", CardType.Default);
}