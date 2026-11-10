using KirisameY.Registration.Data;

namespace BabelRushR.Core.EntityAction.Types;

public class DelegateEntityActionType(RegKey id, Action<DelegateEntityAction, EntityActionContext> action) : IEntityActionType
{
    public RegKey ID => id;

    public IEntityAction CreateAction(int value)
    {
        return new DelegateEntityAction(action);
    }
}

public class ValuedDelegateEntityActionType(RegKey id, Action<ValuedDelegateEntityAction, EntityActionContext, int> action) : IEntityActionType
{
    public RegKey ID => id;

    public IEntityAction CreateAction(int value)
    {
        return new ValuedDelegateEntityAction(action, value);
    }
}