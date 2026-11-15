using BabelRushR.Core.Common;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Level;

public interface IRoomDefinition
{
    [Immutable] RegKey ID { get; }

    [Immutable] double Length { get; }

    IEnumerable<RoomContent> CreateContents(RoomContentContext context);
}
