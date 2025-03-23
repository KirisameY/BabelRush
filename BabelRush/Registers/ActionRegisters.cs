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
    public static IRegister<RegKey, ActionStep> ActionSteps { get; } =
        CreateSimpleRegister.Script<ActionStep, ActionStepModel>("action_steps", ActionStep.Default);

    public static IRegister<RegKey, NameDesc> ActionNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>("actions", "en", id => (id, ""));

    public static IRegister<RegKey, Texture2D> ActionIcon { get; } =
        CreateSimpleRegister.Res<Texture2D, Texture2DModel>("textures/actions", new PlaceholderTexture2D());

    public static IRegister<RegKey, ActionType> Actions { get; } =
        CreateSimpleRegister.Data<ActionType, ActionTypeModel>("actions", ActionType.Default);
}