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
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string Features = "card_features";
        public const string FeatureIcon = $"{SpriteInfoRegisters.Paths.Textures}/{Features}";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, NameDesc> FeatureNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>(Paths.Features, BabelRush.I18n.DefaultLocal, id => (id, ""));

    public static IRegister<RegKey, Texture2D> FeatureIcon { get; } =
        SubRegister.Get(SpriteInfoRegisters.Textures, Paths.Features);

    public static IRegister<RegKey, FeatureType> Features { get; } =
        CreateSimpleRegister.Data<FeatureType, FeatureTypeModel>(Paths.Features, FeatureType.Default);
}