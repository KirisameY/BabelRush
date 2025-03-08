using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

public static class ActionRegisters
{
    public static IRegister<ActionStep> ActionSteps { get; }
    public static IRegister<NameDesc> ActionNameDesc { get; }
    public static IRegister<Texture2D> ActionIcon { get; }
    public static IRegister<ActionType> Actions { get; }

    // [DataRegister<ActionStepModel, ActionStep>("action_steps")]
    // private static readonly CommonRegister<ActionStep> ActionStepsRegister =
    //     new(_ => ActionStep.Default);
    //
    // [LangRegister<NameDescModel, NameDesc>("actions")]
    // private static readonly LocalizedRegister<NameDesc> ActionNameDescRegister =
    //     new("en", id => (id, ""));
    //
    // [ResRegister<Texture2DModel, Texture2D>("textures/actions")]
    // private static readonly CommonRegister<Texture2D> ActionIconRegister =
    //     new(_ => new PlaceholderTexture2D());
    //
    // [DataRegister<ActionTypeModel, ActionType>("actions", "action_steps")]
    // private static readonly CommonRegister<ActionType> ActionsRegister =
    //     new(_ => ActionType.Default);
}