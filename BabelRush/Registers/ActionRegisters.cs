using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registering.Registers;

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
        SubRegister.Create(SpriteInfoRegisters.Textures, "actions");

    public static IRegister<RegKey, ActionType> Actions { get; } =
        CreateSimpleRegister.Data<ActionType, ActionTypeModel>("actions", ActionType.Default);
}