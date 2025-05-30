using System;
using System.Collections.Generic;

using BabelRush.Scenery.Collision;
using BabelRush.Scenery.Rooms;

using Godot;

using KirisameLib.Extensions;
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

    public bool Disposed { get; private set; }

    public void Dispose()
    {
        if (Disposed) return;

        Disposed = true;
        CollisionSpace.Dispose();
        Node.QueueFree();
    }

    #endregion


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
        ObjectDisposedException.ThrowIf(Disposed, this);

        const string logProcess = "AddingRoom";
        if (toRight)
        {
            _rooms.AddLast(room);
            room.Position =  _rightEdge;
            _rightEdge    += room.Length;
            Logger.Log(LogLevel.Info, logProcess, $"Added room {room} to the right, new right edge: {_rightEdge}");
        }
        else
        {
            _rooms.AddFirst(room);
            _leftEdge     -= room.Length;
            room.Position =  _leftEdge;
            Logger.Log(LogLevel.Info, logProcess, $"Added room {room} to the left, new left edge: {_leftEdge}");
        }

        //setup room
        Node.AddChild(Gui.Scenery.RoomInterface.GetInstance(room.Position, room.Length));

        room.Objects.SelectSelf(obj => obj.Position += room.Position).ForEach(AddObject);
    }


    //Collision
    public CollisionSpace CollisionSpace { get; } = new();


    //Objects
    private readonly HashSet<SceneObject> _objects = new();
    private readonly Dictionary<SceneObject, Node> _objectInterfaces = new();

    public void AddObject(SceneObject obj)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!_objects.Add(obj)) return;

        if (obj.Scene is { } scene) scene.RemoveObject(obj);
        obj.Scene = this;

        if (obj is { Collidable: true })
            CollisionSpace.AddObject(obj);

        if (obj is VisualObject vObj)
        {
            AddVisualObject(vObj);
        }

        obj.EnterScene();
    }

    public void RemoveObject(SceneObject obj)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!_objects.Remove(obj)) return;

        obj.ExitScene();

        CollisionSpace.RemoveObject(obj);

        _ = TryRemoveVisualObject(obj);

        obj.Scene = null;
    }

    private readonly SortedList<float, Parallax2D> _layers = [];

    private void AddVisualObject(VisualObject vObj)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (_objectInterfaces.ContainsKey(vObj)) return;

        if (!_layers.TryGetValue(vObj.Parallax, out var layer))
        {
            _layers[vObj.Parallax] = layer = new Parallax2D
            {
                ScrollScale  = new(vObj.Parallax, 1),
                ScrollOffset = Vector2.Zero with { X = Project.ViewportSize.X / 2 }
            };

            Node.AddChild(layer);
            Node.MoveChild(layer, _layers.IndexOfValue(layer));
        }

        var objI = vObj.CreateInterface();
        _objectInterfaces.Add(vObj, objI);
        layer.AddChild(objI);
    }

    private bool TryRemoveVisualObject(SceneObject obj)
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        if (!_objectInterfaces.Remove(obj, out var objI)) return false;
        objI.QueueFree();
        return true;
    }


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(Scene));
}