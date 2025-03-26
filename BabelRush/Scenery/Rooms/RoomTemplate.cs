using System.Collections.Generic;
using System.Collections.Immutable;

using BabelRush.Data;

namespace BabelRush.Scenery.Rooms;

public class RoomTemplate(RegKey id, int length, IEnumerable<(RoomObject obj, double pos)> objects)
{
    public RegKey Id { get; } = id;
    public int Length { get; } = length;
    public ImmutableList<(RoomObject obj, double pos)> Objects { get; } = objects.ToImmutableList();

    public Room CreateRoom() => new Room(Length);

    public static RoomTemplate Default { get; } = new("default", 1, []);
}