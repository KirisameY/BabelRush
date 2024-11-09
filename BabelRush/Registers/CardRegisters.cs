using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static partial class CardRegisters
{
    [LangRegister<NameDescModel, NameDesc>("cards")]
    private static readonly LocalizedRegister<NameDesc> CardNameDescRegister =
        new(nameof(CardNameDescRegister), "en", id => (id, ""));

    [ResRegister<Texture2DModel, Texture2D>("textures/cards")]
    private static readonly CommonRegister<Texture2D> CardIconRegister =
        new(nameof(CardIconRegister), _ => new PlaceholderTexture2D());

    [DataRegister<CardTypeModel, CardType>("cards", "card_features", "actions")]
    private static readonly CommonRegister<CardType> CardsRegister =
        new(nameof(CardsRegister), _ => CardType.Default);
}