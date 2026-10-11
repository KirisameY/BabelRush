namespace BabelRushR.Core.EntityAction;

public class DelegateEntityAction(Action<EntityActionContext> action) : IEntityAction
{
    public void Act(EntityActionContext ctx) => action.Invoke(ctx);
}

public class ValuedDelegateEntityAction(Action<EntityActionContext, int> action, int value) : IEntityAction
{
    public void Act(EntityActionContext ctx) => action.Invoke(ctx, value);
}