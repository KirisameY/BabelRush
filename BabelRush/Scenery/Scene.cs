using System;
using System.Collections.Generic;

using BabelRush.Collision;

using KirisameLib.Logging;

namespace BabelRush.Scenery;

public sealed class Scene : IDisposable
{
    //Initialize&Cleanup
    public void Dispose()
    {
        CollisionSpace.Dispose();
    }


    //Rooms
    private int _leftEdge = 0;
    private int _rightEdge = 0;
    private readonly LinkedList<Room> _rooms = [];

    /// <summary>
    /// Adds a room to the scene at the specified position.
    /// </summary>
    /// <param name="room">The room to be added.</param>
    /// <param name="toRight">Indicates whether the room should be added to the right or left of the current rooms.</param>
    public void AddRoom(Room room, bool toRight)
    {
        const string logProcess = "AddingRoom";
        if (toRight)
        {
            _rooms.AddLast(room);
            room.Position = _rightEdge;
            _rightEdge += room.Length;
            Logger.Log(LogLevel.Info, logProcess, $"Added room {room} to the right, new right edge: {_rightEdge}");
        }
        else
        {
            _rooms.AddFirst(room);
            _leftEdge -= room.Length;
            room.Position = _leftEdge;
            Logger.Log(LogLevel.Info, logProcess, $"Added room {room} to the left, new left edge: {_leftEdge}");
        }
    }


    //Collision
    public CollisionSpace CollisionSpace { get; } = new();


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(Scene));
}