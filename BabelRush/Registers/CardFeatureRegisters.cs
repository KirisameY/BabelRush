using BabelRush.Cards.Features;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

namespace BabelRush.Registers;

public static class CardFeatureRegisters
{
    public static IRegister<string> FeatureName { get; } =
        new I18nRegister<string>(nameof(FeatureName), id => id);
    public static IRegister<string> FeatureDesc { get; } =
        new I18nRegister<string>(nameof(FeatureDesc), _ => "");
    public static IRegister<Texture2D> FeatureIcon { get; } =
        new I18nRegister<Texture2D>(nameof(FeatureIcon), _ => new PlaceholderTexture2D());
    public static IRegister<FeatureType> Features { get; } =
        new CommonRegister<FeatureType>(nameof(Features), _ => FeatureType.Default);
}