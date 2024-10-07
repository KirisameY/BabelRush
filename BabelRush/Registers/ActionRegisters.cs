using System;
using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Registering;

using Godot;

using JetBrains.Annotations;

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

    [RegistrationMap] [UsedImplicitly]
    private static DataRegTool[] DataRegTools { get; } =
    [
        DataRegTool.Get<ActionStep, ActionStepData>("action_steps", ActionStepsRegister),
        DataRegTool.Get<ActionType, ActionTypeData>("actions", ActionsRegister, "action_steps")
    ];

    [RegistrationMap] [UsedImplicitly]
    private static ResRegTool[] ResRegTools { get; } =
    [
        ResRegTool.Get("textures/actions", (_, _) => throw new NotImplementedException(),
                       ActionIconDefaultRegister, ActionIconLocalizedRegister),
    ];

    [RegistrationMap] [UsedImplicitly]
    private static LangRegTool[] LangRegTools { get; } =
    [
        LangRegTool.Get<NameDesc, IDictionary<string, object>>("actions", NameDesc.FromEntry, ActionNameDescRegister),
    ];

    #endregion
}