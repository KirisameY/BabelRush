using System;
using System.Collections.Immutable;

namespace BabelRush.Scenery.Rooms;

public class RoomTemplate(ImmutableList<(Func<SceneObject> obj, int pos)> objects, int length)
{
    public int Length { get; } = length;
    public ImmutableList<(Func<SceneObject> obj, int pos)> Objects { get; } = objects;

    public Room CreateRoom() => new Room(Length);
}