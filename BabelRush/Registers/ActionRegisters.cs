using System.Collections.Frozen;
using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Misc;
using BabelRush.Registering;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

namespace BabelRush.Registers;

[RegisterContainer]
internal static class ActionRegisters
{
    #region Private Registers

    private static readonly CommonRegister<ActionStep> ActionStepsRegister =
        new(nameof(ActionStepsRegister), _ => ActionStep.Default);

    private static readonly LocalizedRegister<NameDesc> ActionNameDescRegister =
        new(nameof(ActionNameDescRegister), "en", id => (id, ""));

    private static readonly CommonRegister<Texture2D> ActionIconDefaultRegister =
        new(nameof(ActionIconDefaultRegister), _ => new PlaceholderTexture2D());

    private static readonly LocalizedRegister<Texture2D> ActionIconLocalizedRegister =
        new(nameof(ActionIconLocalizedRegister), ActionIconDefaultRegister);

    private static readonly CommonRegister<ActionType> ActionsRegister =
        new(nameof(ActionsRegister), _ => ActionType.Default);

    #endregion


    #region Public Registers

    public static IRegister<ActionStep> ActionSteps => ActionStepsRegister;

    public static IRegister<NameDesc> ActionNameDesc => ActionNameDescRegister;

    public static IRegister<Texture2D> ActionIcon => ActionIconLocalizedRegister;

    public static IRegister<ActionType> Actions => ActionsRegister;

    #endregion


    #region Map

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.ParseAndRegisterDelegate> AssetRegisteringMap { get; } =
        new Dictionary<string, Registration.ParseAndRegisterDelegate>
        {
            // Action Steps
            ["data/action_steps"] = Registration.GetRegFunc<IDictionary<string, object>, ActionStepData>
                (
                 ActionStepData.FromEntry,
                 a => ActionStepsRegister.RegisterItem(a.Id, a.ToActionStep())
                ),
            // Actions
            ["data/actions"] = Registration.GetRegFunc<IDictionary<string, object>, ActionTypeData>
                (ActionTypeData.FromEntry,
                 a => ActionsRegister.RegisterItem(a.Id, a.ToActionType()),
                 "data/action_steps"
                ),
            // Action Icons
            ["res/textures/actions"] = null!, //todo: get it to ActionIconDefault
        }.ToFrozenDictionary();

    [RegistrationMap]
    private static FrozenDictionary<string, Registration.LocalizedParseAndRegisterDelegate> LocalRegisteringMap { get; } =
        new Dictionary<string, Registration.LocalizedParseAndRegisterDelegate>
        {
            // Action NameDesc
            ["lang/actions"] = Registration.GetLocalizedRegFunc<KeyValuePair<string, object>, NameDesc>
                (DataUtils.ParseNameDescAndId,
                 (local, id, nd) => ActionNameDescRegister.RegisterLocalizedItem(local, id, nd)
                ),
            // Action Icons
            ["res/textures/actions"] = null!, //todo: get it to ActionIconLocalized
        }.ToFrozenDictionary();

    #endregion
}