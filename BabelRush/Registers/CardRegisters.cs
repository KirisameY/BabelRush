using System.Collections.Frozen;
using System.Collections.Generic;

using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Misc;
using BabelRush.Registering;

using Godot;

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

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.ParseAndRegisterDelegate> RegMap { get; } =
        new Dictionary<string, Registration.ParseAndRegisterDelegate>
        {
            // Cards
            ["data/cards"] = Registration.GetRegFunc<IDictionary<string, object>, CardTypeData>
                (CardTypeData.FromEntry,
                 data => CardsRegister.RegisterItem(data.Id, data.ToCardType()),
                 "data/card_features", "data/actions"
                ),

            // Card Icons
            ["res/textures/cards"] = null! //todo
        }.ToFrozenDictionary();

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.LocalizedParseAndRegisterDelegate> LocalRegMap { get; } =
        new Dictionary<string, Registration.LocalizedParseAndRegisterDelegate>
        {
            // Card NameDesc
            ["lang/cards"] = Registration.GetLocalizedRegFunc<KeyValuePair<string, object>, NameDesc>
                (DataUtils.ParseNameDescAndId,
                 (local, id, nd) => CardNameDescRegister.RegisterLocalizedItem(local, id, nd)
                ),

            // Card Icons
            ["res/textures/cards"] = null! //todo
        }.ToFrozenDictionary();

    #endregion
}