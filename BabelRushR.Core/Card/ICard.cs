using System.ComponentModel;

using BabelRushR.Core.EntityAction;
using BabelRushR.Core.Numeric;

using KirisameY.Numeric;

namespace BabelRushR.Core.Card;

public interface ICard : INotifyPropertyChanged // use empty string for all property changed
{
    IModifierEditableNumeric<int, CommonNumericModifyOrder> Cost { get; }

    IReadOnlyList<IEntityAction> Actions { get; }
}