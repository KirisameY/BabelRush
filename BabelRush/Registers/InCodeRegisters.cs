using System;

using BabelRush.Actions;
using BabelRush.Registering;

using KirisameLib.Data.Registers;

namespace BabelRush.Registers;

[RegisterContainer]
public static class InCodeRegisters
{
    // todo: implement  和Action一起重置了吧

    public static IRegister<ActionDelegate> ActionDelegates => throw new NotImplementedException();

    // public static IRegister<ActionDelegate> ActionDelegates { get; } =
    //     new CommonRegister<ActionDelegate>(_ => (_, _, _) => { });
}