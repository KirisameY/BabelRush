using System.Collections.Generic;

namespace BabelRush.Scenery;

public class Scene
{
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
        if (toRight)
        {
            _rooms.AddLast(room);
            room.Position = _rightEdge;
            _rightEdge += room.Length;
        }
        else
        {
            _rooms.AddFirst(room);
            _leftEdge -= room.Length;
            room.Position = _leftEdge;
        }
    }
}