using BabelRushR.Core.Common;

namespace BabelRushR.Core.EntityAction;

public class DelegateEntityAction(Action<EntityActionContext> action) : ObservableObject, IEntityAction
{
    public void Act(EntityActionContext ctx) => action.Invoke(ctx);
}

public class ValuedDelegateEntityAction(Action<EntityActionContext, int> action, int value) : ObservableObject, IEntityAction
{
    public void Act(EntityActionContext ctx) => action.Invoke(ctx, value);
}