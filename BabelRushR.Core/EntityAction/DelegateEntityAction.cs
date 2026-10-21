using BabelRushR.Core.Common;
using BabelRushR.Core.Numeric;

using KirisameY.GenericUtils;
using KirisameY.Numeric;

namespace BabelRushR.Core.EntityAction;

public class DelegateEntityAction(Action<EntityActionContext> action) : ObservableObject, IEntityAction
{
    public void Act(EntityActionContext ctx) => action.Invoke(ctx);
}

public class ValuedDelegateEntityAction(Action<EntityActionContext, int> action, int value) : ObservableObject, IValuedEntityAction
{
    public IModifierEditableNumeric<int, CommonNumericModifyOrder> Value =>
        INumeric.CreateReadonly(value, TypeA.Of<CommonNumericModifyOrder>())
                .WithUpdateHandler(PropertyChangedHandler());
    public void Act(EntityActionContext ctx) => action.Invoke(ctx, Value.Value);
}