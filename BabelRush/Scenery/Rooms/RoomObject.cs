using System;
using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Mobs;
using BabelRush.Registers;

namespace BabelRush.Scenery.Rooms;

public abstract record RoomObject
{
    public abstract IEnumerable<SceneObject> CreateObject();


    /// <exception cref="ArgumentException"></exception>
    public static RoomObject FromString(string from) => from.ToLower().Split('.') switch
    {
        // todo: 这里有空想办法改一下吧
        ["Mob", var id, var alignment] => new Mob(id, Enum.Parse<Alignment>(alignment)),
        _                              => throw new ArgumentOutOfRangeException(nameof(from), from)
    };


    #region Implements

    public sealed record Mob(RegKey Id, Alignment Alignment) : RoomObject
    {
        public override IEnumerable<SceneObject> CreateObject() => [MobRegisters.Mobs[Id].GetInstance(Alignment)];
    }

    #endregion
}