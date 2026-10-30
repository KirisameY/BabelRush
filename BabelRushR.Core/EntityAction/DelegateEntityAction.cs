using BabelRushR.Core.Common;
using BabelRushR.Core.Numeric;
using BabelRushR.Core.Numeric.CommonModifiers;

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
                .WithModifier(new CommonLimitModifier(min: 0))
                .WithUpdateHandler(PropertyChangedHandler());
    public void Act(EntityActionContext ctx) => action.Invoke(ctx, Value.Value);
}