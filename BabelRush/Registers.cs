using BabelRush.Actions;
using BabelRush.Cards;
using BabelRush.Cards.Features;

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
        new CommonRegister<IActionType>(nameof(Actions), _ => CommonActionType.Default);

    //Feature
    public static IRegister<string> FeatureName { get; } =
        new I18nRegister<string>(nameof(FeatureName), id => id);
    public static IRegister<string> FeatureDesc { get; } =
        new I18nRegister<string>(nameof(FeatureDesc), _ => "");
    public static IRegister<Texture2D> FeatureIcon { get; } =
        new I18nRegister<Texture2D>(nameof(FeatureIcon), _ => new PlaceholderTexture2D());
    public static IRegister<IFeatureType> Features { get; } =
        new CommonRegister<IFeatureType>(nameof(Features), _ => CommonFeatureType.Default);

    //Card
    public static IRegister<string> CardName { get; } =
        new I18nRegister<string>(nameof(CardName), id => id);
    public static IRegister<string> CardDesc { get; } =
        new I18nRegister<string>(nameof(CardDesc), _ => "");
    public static IRegister<Texture2D> CardIcon { get; } =
        new I18nRegister<Texture2D>(nameof(CardIcon), _ => new PlaceholderTexture2D());
    public static IRegister<ICardType> Cards { get; } =
        new CommonRegister<ICardType>(nameof(Cards), _ => CommonCardType.Default);
}