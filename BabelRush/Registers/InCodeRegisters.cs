using BabelRush.Actions;
using BabelRush.Registering;

using KirisameLib.Data.Register;

namespace BabelRush.Registers;

[RegisterContainer]
public static partial class InCodeRegisters
{
    public static IRegister<ActionDelegate> ActionDelegates { get; } =
        new CommonRegister<ActionDelegate>(nameof(ActionDelegates), _ => (_, _, _) => { });
}