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
        SimpleRegisterCreate.Lang<NameDesc, NameDescModel>("card_features", "en", id => (id, ""));

    public static IRegister<Texture2D> FeatureIcon { get; } =
        SimpleRegisterCreate.Res<Texture2D, Texture2DModel>("textures/card_features", new PlaceholderTexture2D());

    public static IRegister<FeatureType> Features { get; } =
        SimpleRegisterCreate.Data<FeatureType, FeatureTypeModel>("card_features", FeatureType.Default);
}