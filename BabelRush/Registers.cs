using BabelRush.Actions;
using BabelRush.Cards;

using KirisameLib.Register;

namespace BabelRush;

public static class Registers
{
    private static IRegister<IAction>? Actions { get; set; }
    private static IRegister<ICardType>? Cards { get; set; }
}