using BabelRushR.Core.Common;
using BabelRushR.Core.Numeric;
using BabelRushR.Core.Numeric.CommonModifiers;

using KirisameY.GenericUtils;
using KirisameY.Numeric;

namespace BabelRushR.Core.EntityAction;

public class DelegateEntityAction(Action<DelegateEntityAction, EntityActionContext> action) : ObservableObject, IEntityAction
{
    public void Act(EntityActionContext ctx) => action.Invoke(this, ctx);
}

public class ValuedDelegateEntityAction(Action<ValuedDelegateEntityAction, EntityActionContext, int> action, int value) : ObservableObject, IValuedEntityAction
{
    public IModifierEditableNumeric<int, CommonNumericModifyOrder> Value =>
        INumeric.CreateReadonly(value, TypeA.Of<CommonNumericModifyOrder>())
                .WithModifier(new CommonLimitModifier(min: 0))
                .WithUpdateHandler(PropertyChangedHandler());
    public void Act(EntityActionContext ctx) => action.Invoke(this, ctx, Value.Value);
}