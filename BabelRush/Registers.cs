using BabelRush.Actions;
using BabelRush.Cards;

using Godot;

using KirisameLib.I18n;
using KirisameLib.Register;

namespace BabelRush;

public static class Registers
{
    //Card
    public static IRegister<string> CardName { get; set; } =
        new I18nRegister<string>(nameof(CardName), id => id);
    public static IRegister<Texture2D> CardIcon { get; set; } =
        new I18nRegister<Texture2D>(nameof(CardIcon), _ => new PlaceholderTexture2D());
    public static IRegister<ICardType> Cards { get; set; } =
        new CommonRegister<ICardType>(nameof(Cards), _ => CardType.Default);
}