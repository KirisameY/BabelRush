namespace BabelRushR.Core.EntityAction;

public interface IEntityActionType
{
    IEntityAction CreateAction(int value);
}