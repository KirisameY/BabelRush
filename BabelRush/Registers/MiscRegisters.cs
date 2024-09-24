using BabelRush.Actions;

using KirisameLib.Core.Register;

namespace BabelRush.Registers;

public static class MiscRegisters
{
    public static IRegister<ActionDelegate> ActionDelegates { get; } =
        new CommonRegister<ActionDelegate>(nameof(ActionDelegates), _ => (_, _, _) => { });

    public static IRegister<ActionItem> ActionItems { get; } =
        new CommonRegister<ActionItem>(nameof(ActionItems), _ => ActionItem.Default);
}