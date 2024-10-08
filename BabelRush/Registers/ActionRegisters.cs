using System;
using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Data.ExtendBoxes;
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
        new DataRegTool<ActionStep, ActionStepBox>("action_steps", ActionStepsRegister),
        new DataRegTool<ActionType, ActionTypeBox>("actions", ActionsRegister, "action_steps")
    ];

    [RegistrationMap] [UsedImplicitly]
    private static ResRegTool[] ResRegTools { get; } =
    [
        new ResRegTool<Texture2D, Texture2DBox>("textures/actions", ActionIconDefaultRegister, ActionIconLocalizedRegister),
    ];

    [RegistrationMap] [UsedImplicitly]
    private static LangRegTool[] LangRegTools { get; } =
    [
        new LangRegTool<NameDesc, NameDescBox>("actions", ActionNameDescRegister),
    ];

    #endregion
}