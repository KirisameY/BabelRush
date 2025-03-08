using BabelRush.Cards;
using BabelRush.Data;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

public static class CardRegisters
{
    public static IRegister<NameDesc> CardNameDesc { get; }
    public static IRegister<Texture2D> CardIcon { get; }
    public static IRegister<CardType> Cards { get; }

    // [LangRegister<NameDescModel, NameDesc>("cards")]
    // private static readonly LocalizedRegister<NameDesc> CardNameDescRegister =
    //     new("en", id => (id, ""));
    //
    // [ResRegister<Texture2DModel, Texture2D>("textures/cards")]
    // private static readonly CommonRegister<Texture2D> CardIconRegister =
    //     new(_ => new PlaceholderTexture2D());
    //
    // [DataRegister<CardTypeModel, CardType>("cards", "card_features", "actions")]
    // private static readonly CommonRegister<CardType> CardsRegister =
    //     new(_ => CardType.Default);
}