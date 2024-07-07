using System;

using BabelRush.Actions;
using BabelRush.Cards;

using Godot;

using KirisameLib.I18n;
using KirisameLib.Register;

namespace BabelRush;

public static class Registers
{
    //Action
    public static IRegister<string> ActionName { get; } =
        new I18nRegister<string>(nameof(ActionName), id => id);
    public static IRegister<string> ActionDesc { get; } =
        new I18nRegister<string>(nameof(ActionDesc), _ => "");
    public static IRegister<Texture2D> ActionIcon { get; } =
        new I18nRegister<Texture2D>(nameof(ActionIcon), _ => new PlaceholderTexture2D());
    public static IRegister<IActionType> Actions { get; } =
        new CommonRegister<IActionType>(nameof(Actions), _ => ActionType.Default);

    //Card
    public static IRegister<string> CardName { get; set; } =
        new I18nRegister<string>(nameof(CardName), id => id);
    public static IRegister<Texture2D> CardIcon { get; set; } =
        new I18nRegister<Texture2D>(nameof(CardIcon), _ => new PlaceholderTexture2D());
    public static IRegister<ICardType> Cards { get; set; } =
        new CommonRegister<ICardType>(nameof(Cards), _ => CardType.Default);
}