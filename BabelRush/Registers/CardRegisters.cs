using BabelRush.Cards;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

namespace BabelRush.Registers;

public static class CardRegisters
{
    public static IRegister<string> CardName { get; } =
        new I18nRegister<string>(nameof(CardName), id => id);
    public static IRegister<string> CardDesc { get; } =
        new I18nRegister<string>(nameof(CardDesc), _ => "");
    public static IRegister<Texture2D> CardIcon { get; } =
        new I18nRegister<Texture2D>(nameof(CardIcon), _ => new PlaceholderTexture2D());
    public static IRegister<CardType> Cards { get; } =
        new CommonRegister<CardType>(nameof(Cards), _ => CardType.Default);
}