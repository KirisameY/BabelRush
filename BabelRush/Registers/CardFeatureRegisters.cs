using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static class CardFeatureRegisters
{
    #region Private Registers

    private static readonly LocalizedRegister<NameDesc> FeatureNameDescRegister =
        new(nameof(FeatureNameDescRegister), "en", id => (id, ""));

    private static readonly CommonRegister<Texture2D> FeatureIconDefaultRegister =
        new(nameof(FeatureIconDefaultRegister), _ => new PlaceholderTexture2D());

    private static readonly LocalizedRegister<Texture2D> FeatureIconLocalizedRegister =
        new(nameof(FeatureIconLocalizedRegister), FeatureIconDefaultRegister);

    private static readonly CommonRegister<FeatureType> FeaturesRegister =
        new(nameof(FeaturesRegister), _ => FeatureType.Default);

    #endregion


    #region Public Registers

    public static IRegister<NameDesc> FeatureNameDesc => FeatureNameDescRegister;

    public static IRegister<Texture2D> FeatureIcon => FeatureIconLocalizedRegister;

    public static IRegister<FeatureType> Features => FeaturesRegister;

    #endregion


    #region Map

    // [RegistrationMap] [UsedImplicitly]
    // private static DataRegTool[] DataRegTools { get; } =
    // [
    //     new DataRegTool<FeatureType, FeatureTypeModel>("card_features", FeaturesRegister),
    // ];
    //
    // [RegistrationMap] [UsedImplicitly]
    // private static ResRegTool[] ResRegTools { get; } =
    // [
    //     new ResRegTool<Texture2D, Texture2DModel>("textures/card_features", FeatureIconDefaultRegister, FeatureIconLocalizedRegister),
    // ];
    //
    // [RegistrationMap] [UsedImplicitly]
    // private static LangRegTool[] LangRegTools { get; } =
    // [
    //     new LangRegTool<NameDesc, NameDescModel>("card_features", FeatureNameDescRegister),
    // ];

    #endregion
}