using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

// todo: all these registers need to be implemented
public static class CardFeatureRegisters
{
    public static IRegister<NameDesc> FeatureNameDesc { get; }
    public static IRegister<Texture2D> FeatureIcon { get; }
    public static IRegister<FeatureType> Features { get; }

    // [LangRegister<NameDescModel, NameDesc>("card_features")]
    // private static readonly LocalizedRegister<NameDesc> FeatureNameDescRegister =
    //     new("en", id => (id, ""));
    //
    // [ResRegister<Texture2DModel, Texture2D>("textures/card_features")]
    // private static readonly CommonRegister<Texture2D> FeatureIconRegister =
    //     new(_ => new PlaceholderTexture2D());
    //
    // [DataRegister<FeatureTypeModel, FeatureType>("card_features")]
    // private static readonly CommonRegister<FeatureType> FeaturesRegister =
    //     new(_ => FeatureType.Default);
}