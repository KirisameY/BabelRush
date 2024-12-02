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
        new(_ => ActionStep.Default);

    [LangRegister<NameDescModel, NameDesc>("actions")]
    private static readonly LocalizedRegister<NameDesc> ActionNameDescRegister =
        new("en", id => (id, ""));

    [ResRegister<Texture2DModel, Texture2D>("textures/actions")]
    private static readonly CommonRegister<Texture2D> ActionIconRegister =
        new(_ => new PlaceholderTexture2D());

    [DataRegister<ActionTypeModel, ActionType>("actions", "action_steps")]
    private static readonly CommonRegister<ActionType> ActionsRegister =
        new(_ => ActionType.Default);
}