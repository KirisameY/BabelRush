using System;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Level.Collision;
using BabelRush.Level.Rooms;
using BabelRush.Level.Scenery;
using BabelRush.Level.Stages.Template;

namespace BabelRush.Level.Stages;

public sealed class Stage(StageTemplate template, StageNode startNode) : IDisposable
{
    #region Dispose

    public void Dispose()
    {
        if (Disposed) return;

        Disposed = true;
        _sceneCleanup?.Invoke();
        Scene.InternalDispose();
        PathSelector.InternalDispose();
    }

    public bool Disposed { get; private set; }

    #endregion


    public StageTemplate Template => template;
    public StageNode StartNode => startNode;

    [field: AllowNull, MaybeNull]
    internal StagePathSelector PathSelector => field ??= new(this);

    [field: AllowNull, MaybeNull]
    public Scene Scene => field ??= CreateScene();

    private Action? _sceneCleanup;


    private Scene CreateScene()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        (StageNode node, Area junctionArea)? currentState = null;
        var createdScene = new Scene(this);

        AddRooms(StartNode);

        var unReg = Game.GameEventBus.SubscribeAsync<ObjectEnteredAreaEvent>(async e =>
        {
            if (createdScene.Disposed) return;
            if (e.Object != Game.Play!.BattleField.Player || e.Area != currentState?.junctionArea) return;

            Game.Play.PlayerState.WantMove = false;
            Game.GameEventBus.Publish(new StagePathChosenRequestEvent(this, currentState.Value.node.NextRooms));
            var nextNode = await PathSelector.GetNextNode();

            AddRooms(nextNode);
            Game.Play.PlayerState.WantMove = true;
        });
        _sceneCleanup += unReg;

        return createdScene;


        void AddRooms(StageNode node)
        {
            while (node.NextRooms is [var n])
            {
                createdScene.AddRoom(node.Room.CreateRoom(), true);
                node = n;
            }
            createdScene.AddRoom(node.Room.CreateRoom(), true);

            PathSelector.SelectableNodes = node.NextRooms;

            if (node.NextRooms is []) return;
            var pos = node.Room.Objects.Find(o => o.obj is RoomObject.Marker { Mark: "junction" }).pos;
            var junctionArea = new Area(pos, 4d);
            currentState = (node, junctionArea);
            createdScene.CollisionSpace.AddArea(junctionArea);
        }
    }


    public static Stage Default { get; } = new(StageTemplate.Default, StageNode.Default);
}