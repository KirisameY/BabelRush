using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class CardFeatureRegisters
{
    public static IRegister<NameDesc> FeatureNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("card_features", "en", id => (id, ""));

    public static IRegister<Texture2D> FeatureIcon { get; } =
        CreateSimpleRegister.Res<Texture2D, Texture2DModel>("textures/card_features", new PlaceholderTexture2D());

    public static IRegister<FeatureType> Features { get; } =
        CreateSimpleRegister.Data<FeatureType, FeatureTypeModel>("card_features", FeatureType.Default);
}