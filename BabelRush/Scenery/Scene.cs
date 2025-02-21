using System;
using System.Collections.Generic;

using BabelRush.Scenery.Collision;
using BabelRush.Scenery.Rooms;

using Godot;

using KirisameLib.Logging;

namespace BabelRush.Scenery;

public sealed class Scene : IDisposable
{
    #region Initialize&Cleanup

    public void Ready(Node parent)
    {
        CollisionSpace.Ready();
        parent.AddChild(Node);
        parent.MoveChild(Node, 0);
        Node.Name = "Scene";
    }

    public void Dispose()
    {
        CollisionSpace.Dispose();
        Node.QueueFree();
    }

    #endregion


    //Node
    public Node2D Node { get; } = new(); //todo: Remove this shit


    //Rooms
    private int _leftEdge = 0;
    private int _rightEdge = 0;
    private readonly LinkedList<Room> _rooms = [];

    /// <summary>
    /// Adds a room to the scene at the specified position.
    /// </summary>
    /// <param name="room">The room to be added.</param>
    /// <param name="toRight">Indicates whether the room should be added to the right or left of the current rooms.</param>
    public void AddRoom(RoomTemplate room, bool toRight)
    {
        const string logProcess = "AddingRoom";
        var r = room.CreateRoom();
        if (toRight)
        {
            _rooms.AddLast(r);
            r.Position = _rightEdge;
            _rightEdge += r.Length;
            Logger.Log(LogLevel.Info, logProcess, $"Added room {r} to the right, new right edge: {_rightEdge}");
        }
        else
        {
            _rooms.AddFirst(r);
            _leftEdge -= r.Length;
            r.Position = _leftEdge;
            Logger.Log(LogLevel.Info, logProcess, $"Added room {r} to the left, new left edge: {_leftEdge}");
        }

        //setup room
        Node.AddChild(Gui.Scenery.RoomInterface.GetInstance(r.Position, r.Length));

        foreach ((RoomObject roomObj, double pos) in room.Objects)
        {
            var obj = roomObj.CreateObject();
            obj.Position = pos + r.Position;
            AddObject(obj);
        }
    }


    //Collision
    public CollisionSpace CollisionSpace { get; } = new();


    //Objects
    private readonly Dictionary<VisualObject, Node> _objectInterfaces = new();

    public void AddObject(SceneObject obj)
    {
        CollisionSpace.AddObject(obj);

        if (obj is not VisualObject vObj) return;
        var objI = vObj.CreateInterface();
        if (_objectInterfaces.TryAdd(vObj, objI))
            Node.AddChild(objI);
    }

    public void RemoveObject(SceneObject obj)
    {
        CollisionSpace.RemoveObject(obj);

        if (obj is not VisualObject vObj) return;
        if (_objectInterfaces.Remove(vObj, out var objI))
            Node.RemoveChild(objI);
    }


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(Scene));
}