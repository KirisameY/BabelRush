using BabelRush.Actions;

using KirisameLib.Core.Register;

namespace BabelRush.Registers;

public static class MiscRegisters
{
    public static IRegister<ActionDelegate> ActionDelegates { get; } =
        new CommonRegister<ActionDelegate>(nameof(ActionDelegates), _ => (_, _, _) => { });
}