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
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Cards = "cards";
        public const string CardIcon = $"{SpriteInfoRegisters.Paths.Sprites}/{Cards}";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, NameDesc> CardNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>(Paths.Cards, BabelRush.I18n.DefaultLocal, id => (id, ""));

    public static IRegister<RegKey, SpriteInfo> CardIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Sprites, Paths.Cards);

    public static IRegister<RegKey, CardType> Cards { get; } =
        CreateSimpleRegister.Data<CardType, CardTypeModel>(Paths.Cards, CardType.Default);
}