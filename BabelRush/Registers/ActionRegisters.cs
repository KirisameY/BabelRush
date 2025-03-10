using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registering;

using Godot;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class ActionRegisters
{
    public static IRegister<ActionStep> ActionSteps { get; } =
        SimpleRegisterCreate.Data<ActionStep, ActionStepModel>("action_steps", ActionStep.Default);

    public static IRegister<NameDesc> ActionNameDesc { get; } =
        SimpleRegisterCreate.Lang<NameDesc, NameDescModel>("actions", "en", id => (id, ""));

    public static IRegister<Texture2D> ActionIcon { get; } =
        SimpleRegisterCreate.Res<Texture2D, Texture2DModel>("textures/actions", new PlaceholderTexture2D());

    public static IRegister<ActionType> Actions { get; } =
        SimpleRegisterCreate.Data<ActionType, ActionTypeModel>("actions", ActionType.Default);
}