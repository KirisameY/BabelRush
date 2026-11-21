using BabelRushR.Core.Common;

using KirisameY.Registration.Data;

using Tomlyn.Serialization;

namespace BabelRushR.Core.Level;

[TomlPolymorphic]
public interface IRoomDefinition
{
    [Immutable] RegKey ID { get; }

    [Immutable] double Length { get; }

    IEnumerable<RoomContent> CreateContents();
}
