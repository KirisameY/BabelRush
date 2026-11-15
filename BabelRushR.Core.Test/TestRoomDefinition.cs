using BabelRushR.Core.Entity;
using BabelRushR.Core.Level;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

internal sealed class TestRoomDefinition : IRoomDefinition
{
    private readonly IEntity[] _contents;

    public TestRoomDefinition(RegKey id, double length, params IEntity[] contents)
    {
        ID = id;
        Length = length;
        _contents = contents;
    }

    public RegKey ID { get; }

    public double Length { get; }

    public IReadOnlyList<IEntity> Contents => _contents;

    public IEnumerable<RoomContent> CreateContents(RoomContentContext context)
    {
        for (var i = 0; i < _contents.Length; i++)
        {
            yield return new RoomContent(_contents[i], i);
        }
    }
}
