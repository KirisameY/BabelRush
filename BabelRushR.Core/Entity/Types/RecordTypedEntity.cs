using KirisameY.Registration.Data;

namespace BabelRushR.Core.Entity.Types;

public class RecordTypedEntity(RecordEntityType type) : EntityBase(type.MaxHP)
{
    public override RegKey DisplayKey => type.ID;
}