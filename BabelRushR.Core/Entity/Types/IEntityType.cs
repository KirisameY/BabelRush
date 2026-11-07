using BabelRushR.Core.Common;

using KirisameY.Registration.Data;

using Tomlyn.Serialization;

namespace BabelRushR.Core.Entity.Types;

[TomlPolymorphic]
[TomlDerivedType(typeof(RecordEntityType))]
public interface IEntityType
{
    [Immutable] public RegKey ID { get; }

    public IEntity CreateEntity();
}