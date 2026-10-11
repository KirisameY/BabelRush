namespace BabelRushR.Core.EntityAction.Types;

public class DelegateEntityActionType(Action<EntityActionContext> action) : IEntityActionType
{
    public IEntityAction CreateAction(int value)
    {
        return new DelegateEntityAction(action);
    }
}

public class ValuedDelegateEntityActionType(Action<EntityActionContext, int> action) : IEntityActionType
{
    public IEntityAction CreateAction(int value)
    {
        return new ValuedDelegateEntityAction(action, value);
    }
}