using System;
using System.Collections.Generic;

using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Registering;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static class CardRegisters
{
    #region Private Registers

    private static readonly LocalizedRegister<NameDesc> CardNameDescRegister =
        new(nameof(CardNameDescRegister), "en", id => (id, ""));

    private static readonly CommonRegister<Texture2D> CardIconDefaultRegister =
        new(nameof(CardIconDefaultRegister), _ => new PlaceholderTexture2D());

    private static readonly LocalizedRegister<Texture2D> CardIconLocalizedRegister =
        new(nameof(CardIconLocalizedRegister), CardIconDefaultRegister);

    private static readonly CommonRegister<CardType> CardsRegister =
        new(nameof(CardsRegister), _ => CardType.Default);

    #endregion


    #region Public Registers

    public static IRegister<NameDesc> CardNameDesc => CardNameDescRegister;

    public static IRegister<Texture2D> CardIcon => CardIconLocalizedRegister;

    public static IRegister<CardType> Cards => CardsRegister;

    #endregion


    #region Map

    [RegistrationMap] [UsedImplicitly]
    private static DataRegTool[] DataRegTools { get; } =
    [
        DataRegTool.Get<CardType, CardTypeData>("cards", CardsRegister, "card_features", "actions"),
    ];

    [RegistrationMap] [UsedImplicitly]
    private static ResRegTool[] ResRegTools { get; } =
    [
        ResRegTool.Get("textures/cards", (_, _) => throw new NotImplementedException(),
                       CardIconDefaultRegister, CardIconLocalizedRegister),
    ];

    [RegistrationMap] [UsedImplicitly]
    private static LangRegTool[] LangRegTools { get; } =
    [
        LangRegTool.Get<NameDesc, IDictionary<string, object>>("cards", NameDesc.FromEntry, CardNameDescRegister),
    ];

    #endregion
}