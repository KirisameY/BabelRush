using BabelRush.Actions;
using BabelRush.Registering;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

public static class InCodeRegisters
{
    public static IRegister<ActionDelegate> ActionDelegates { get; }

    // public static IRegister<ActionDelegate> ActionDelegates { get; } =
    //     new CommonRegister<ActionDelegate>(_ => (_, _, _) => { });
}