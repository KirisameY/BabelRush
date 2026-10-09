using System.ComponentModel;

using BabelRushR.Core.EntityAction;
using BabelRushR.Core.Numeric;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.Numeric;

namespace BabelRushR.Core.Card;

public interface ICard : INotifyPropertyChanged // use empty string for all property changed
{
    string DisplayKey { get; }

    IModifierEditableNumeric<int, CommonNumericModifyOrder> Cost { get; }

    IReadOnlyNotifiableList<IEntityAction> Actions { get; }
}