using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Data.ExtendBoxes;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static partial class CardFeatureRegisters
{
    [LangRegister<NameDescModel, NameDesc>("card_features")]
    private static readonly LocalizedRegister<NameDesc> FeatureNameDescRegister =
        new(nameof(FeatureNameDescRegister), "en", id => (id, ""));

    [ResRegister<Texture2DModel, Texture2D>("textures/card_features")]
    private static readonly CommonRegister<Texture2D> FeatureIconRegister =
        new(nameof(FeatureIconRegister), _ => new PlaceholderTexture2D());

    [DataRegister<FeatureTypeModel, FeatureType>("card_features")]
    private static readonly CommonRegister<FeatureType> FeaturesRegister =
        new(nameof(FeaturesRegister), _ => FeatureType.Default);
}