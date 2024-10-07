using System;
using System.Collections.Generic;

using BabelRush.Cards.Features;
using BabelRush.Data;
using BabelRush.Registering;

using Godot;

using JetBrains.Annotations;

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

    [RegistrationMap] [UsedImplicitly]
    private static DataRegTool[] DataRegTools { get; } =
    [
        DataRegTool.Get<FeatureType, FeatureTypeData>("card_features", FeaturesRegister),
    ];

    [RegistrationMap] [UsedImplicitly]
    private static ResRegTool[] ResRegTools { get; } =
    [
        ResRegTool.Get("textures/card_features", (_, _) => throw new NotImplementedException(),
                       FeatureIconDefaultRegister, FeatureIconLocalizedRegister),
    ];

    [RegistrationMap] [UsedImplicitly]
    private static LangRegTool[] LangRegTools { get; } =
    [
        LangRegTool.Get<NameDesc, IDictionary<string, object>>("card_features", NameDesc.FromEntry, FeatureNameDescRegister),
    ];

    #endregion
}