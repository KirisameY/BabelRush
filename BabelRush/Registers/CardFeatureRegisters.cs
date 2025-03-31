using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registering.Registers;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class CardFeatureRegisters
{
    public static IRegister<RegKey, NameDesc> FeatureNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("card_features", "en", id => (id, ""));

    public static IRegister<RegKey, Texture2D> FeatureIcon { get; } =
        SubRegister.Create(SpriteInfoRegisters.Textures, "card_features");

    public static IRegister<RegKey, FeatureType> Features { get; } =
        CreateSimpleRegister.Data<FeatureType, FeatureTypeModel>("card_features", FeatureType.Default);
}