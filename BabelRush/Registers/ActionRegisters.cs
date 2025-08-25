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
    public static class Paths
    {
        // ReSharper disable MemberHidesStaticFromOuterClass
        public const string ActionSteps = "action_steps";
        public const string Actions = "actions";
        public const string ActionIcon = $"{SpriteInfoRegisters.Paths.Textures}/{Actions}";
        // ReSharper restore MemberHidesStaticFromOuterClass
    }


    public static IRegister<RegKey, ActionStep> ActionSteps { get; } =
        CreateSimpleRegister.Script<ActionStep, ActionStepModel>(Paths.ActionSteps, ActionStep.Default);

    public static IRegister<RegKey, NameDesc> ActionNameDesc { get; } =
        CreateSimpleRegister.Lang<NameDesc, NameDescModel>(Paths.Actions, BabelRush.I18n.DefaultLocal, id => (id, ""));

    public static IRegister<RegKey, Texture2D> ActionIcon { get; } =
        SubRegister.Get(SpriteInfoRegisters.Textures, Paths.Actions);

    public static IRegister<RegKey, ActionType> Actions { get; } =
        CreateSimpleRegister.Data<ActionType, ActionTypeModel>(Paths.Actions, ActionType.Default);
}