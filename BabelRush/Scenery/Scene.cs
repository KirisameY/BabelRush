using System;
using System.Collections.Generic;

using KirisameLib.Logging;

namespace BabelRush.Scenery;

public class Scene
{
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
    

    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("Scene");
}