using KirisameY.Registration.Data;

namespace BabelRushR.Core.EntityAction.Types;

public interface IEntityActionType
{
    RegKey ID { get; }
    IEntityAction CreateAction(int value);
}