using System.ComponentModel;

using BabelRushR.Core.EntityAction;
using BabelRushR.Core.Numeric;

using KirisameY.NotifiableCollections.Collections;
using KirisameY.Numeric;
using KirisameY.Registration.Data;

namespace BabelRushR.Core.Card;

public interface ICard : INotifyPropertyChanged // use empty string for all property changed
{
    RegKey DisplayKey { get; }

    IModifierEditableNumeric<int, CommonNumericModifyOrder> Cost { get; }

    IReadOnlyNotifiableList<IEntityAction> Actions { get; }
}