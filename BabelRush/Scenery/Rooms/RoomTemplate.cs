using System.Collections.Generic;
using System.Collections.Immutable;

namespace BabelRush.Scenery.Rooms;

public class RoomTemplate(string id, int length, IEnumerable<(RoomObject obj, double pos)> objects)
{
    public string Id { get; } = id;
    public int Length { get; } = length;
    public ImmutableList<(RoomObject obj, double pos)> Objects { get; } = objects.ToImmutableList();

    public Room CreateRoom() => new Room(Length);

    public static RoomTemplate Default { get; } = new("default", 1, []);
}