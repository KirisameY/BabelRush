namespace BabelRushR.Core.EntityAction.Types;

public interface IEntityActionType
{
    IEntityAction CreateAction(int value);
}