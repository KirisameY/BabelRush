using System.Collections.Frozen;
using System.Collections.Generic;

using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Misc;
using BabelRush.Registering;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

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

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.ParseAndRegisterDelegate> AssetRegisteringMap { get; } =
        new Dictionary<string, Registration.ParseAndRegisterDelegate>
        {
            // Card Features
            ["data/card_features"] = Registration.GetRegFunc<IDictionary<string, object>, FeatureTypeData>
                (FeatureTypeData.FromEntry,
                 data => FeaturesRegister.RegisterItem(data.Id, data.ToFeatureType())
                ),
            // Icons
            ["res/textures/card_features"] = null! //todo
        }.ToFrozenDictionary();

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.LocalizedParseAndRegisterDelegate> LocalAssetRegisteringMap { get; } =
        new Dictionary<string, Registration.LocalizedParseAndRegisterDelegate>
        {
            // NameDesc
            ["lang/card_features"] = Registration.GetLocalizedRegFunc<KeyValuePair<string, object>, NameDesc>
                (
                 DataUtils.ParseNameDescAndId,
                 (local, id, nameDesc) => FeatureNameDescRegister.RegisterLocalizedItem(local, id, nameDesc)
                ),
            //Icons
            ["res/textures/card_features"] = null! //todo
        }.ToFrozenDictionary();

    #endregion
}