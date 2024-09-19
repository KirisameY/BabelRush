using BabelRush.Actions;

using Godot;

using KirisameLib.Core.I18n;
using KirisameLib.Core.Register;

namespace BabelRush.Registers;

public static class ActionRegisters
{
    public static IRegister<string> ActionName { get; } =
        new I18nRegister<string>(nameof(ActionName), id => id);
    public static IRegister<string> ActionDesc { get; } =
        new I18nRegister<string>(nameof(ActionDesc), _ => "");
    public static IRegister<Texture2D> ActionIcon { get; } =
        new I18nRegister<Texture2D>(nameof(ActionIcon), _ => new PlaceholderTexture2D());
    public static IRegister<ActionType> Actions { get; } =
        new CommonRegister<ActionType>(nameof(Actions), _ => ActionType.Default);
}