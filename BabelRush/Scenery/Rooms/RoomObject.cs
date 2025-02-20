using System;

using BabelRush.Mobs;
using BabelRush.Registers;

namespace BabelRush.Scenery.Rooms;

public abstract record RoomObject
{
    public abstract SceneObject CreateObject();


    /// <exception cref="ArgumentException"></exception>
    public static RoomObject FromString(string from) => from.ToLower().Split('.') switch
    {
        ["Mob", var id, var alignment] => new Mob(id, Enum.Parse<Alignment>(alignment)),
        _                              => throw new ArgumentOutOfRangeException(nameof(from), from)
    };


    #region Implements

    public sealed record Mob(string Id, Alignment Alignment) : RoomObject
    {
        public override SceneObject CreateObject() => MobRegisters.Mobs[Id].GetInstance(Alignment);
    }

    #endregion
}