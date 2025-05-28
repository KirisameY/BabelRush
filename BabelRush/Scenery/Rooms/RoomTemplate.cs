using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registers;

using Godot;

using KirisameLib.Extensions;

namespace BabelRush.Scenery.Rooms;

public class RoomTemplate(RegKey id, int length, IEnumerable<(RoomObject obj, double pos)> objects)
{
    public RegKey Id => id;
    public Texture2D Icon => LevelRegisters.RoomIcon[id]; // todo: 应该坐上某种回退机制的
    public int Length => length;
    public ImmutableList<(RoomObject obj, double pos)> Objects { get; } = objects.ToImmutableList();

    public Room CreateRoom() => new Room(
        Length,
        Objects.SelectMany(t => t.obj.CreateObject().SelectSelf(obj => obj.Position += t.pos)).ToImmutableArray()
    );

    public static RoomTemplate Default { get; } = new(RegKey.Default, 1, []);
}