using KirisameY.Registration.Data;

namespace BabelRushR.Core.Entity.Types;

public record RecordEntityType(RegKey ID, int MaxHP) : IEntityType
{
    public IEntity CreateEntity() => new RecordTypedEntity(this);
}