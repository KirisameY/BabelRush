using System.ComponentModel;

using BabelRushR.Core.Numeric;

using KirisameY.Numeric;

namespace BabelRushR.Core.EntityAction;

public interface IEntityAction : INotifyPropertyChanged
{
    void Act(EntityActionContext ctx);
}

public interface IValuedEntityAction : IEntityAction
{
    IModifierEditableNumeric<int, CommonNumericModifyOrder> Value { get; }
}