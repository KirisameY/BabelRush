using BabelRushR.Core.Entity;
using BabelRushR.Core.Level;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

internal sealed class TestRoomDefinition(RegKey id, double length, params IEntity[] contents) : IRoomDefinition
{
    public RegKey ID => id;

    public double Length => length;

    public IReadOnlyList<IEntity> Contents => contents;

    public IEnumerable<RoomContent> CreateContents()
    {
        for (var i = 0; i < contents.Length; i++)
        {
            yield return new RoomContent(contents[i], i);
        }
    }
}
