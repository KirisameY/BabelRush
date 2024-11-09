using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.I18n;
using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
internal static partial class ActionRegisters
{
    [DataRegister<ActionStepModel, ActionStep>("action_steps")]
    private static readonly CommonRegister<ActionStep> ActionStepsRegister =
        new(nameof(ActionStepsRegister), _ => ActionStep.Default);

    [LangRegister<NameDescModel, NameDesc>("actions")]
    private static readonly LocalizedRegister<NameDesc> ActionNameDescRegister =
        new(nameof(ActionNameDescRegister), "en", id => (id, ""));

    [ResRegister<Texture2DModel, Texture2D>("textures/actions")]
    private static readonly CommonRegister<Texture2D> ActionIconRegister =
        new(nameof(ActionIconRegister), _ => new PlaceholderTexture2D());

    [DataRegister<ActionTypeModel, ActionType>("actions", "action_steps")]
    private static readonly CommonRegister<ActionType> ActionsRegister =
        new(nameof(ActionsRegister), _ => ActionType.Default);
}