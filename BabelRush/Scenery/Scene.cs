using System;
using System.Collections.Generic;

using BabelRush.GamePlay;
using BabelRush.Mobs;
using BabelRush.Scenery.Collision;

using Godot;

using KirisameLib.Logging;

namespace BabelRush.Scenery;

public sealed class Scene : IDisposable
{
    //Initialize&Cleanup
    public void Ready()
    {
        CollisionSpace.Ready();
        Play.Node.AddChild(Node);
        Play.Node.MoveChild(Node, 0);
        Node.Name = "Scene";
    }

    public void Dispose()
    {
        CollisionSpace.Dispose();
        Node.QueueFree();
    }


    //Node
    public Node2D Node { get; } = new();


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

        //setup room
        Node.AddChild(RoomNode.GetInstance(room.Position, room.Length));

        foreach ((MobType mobType1, Alignment alignment1, int pos1) in room.Mobs)
        {
            var mob1 = mobType1.GetInstance(alignment1);
            mob1.Position = pos1 + room.Position;
            AddObject(mob1);
        }
    }


    //Collision
    public CollisionSpace CollisionSpace { get; } = new();


    //Objects
    public void AddObject(SceneObject obj)
    {
        CollisionSpace.AddObject(obj);
        Node.AddChild(obj.CreateInterface());
    }

    public void RemoveObject(SceneObject obj)
    {
        CollisionSpace.RemoveObject(obj);
        Node.RemoveChild(obj.CreateInterface());
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(Scene));
}