using System;
using System.Threading.Tasks;

using BabelRush.Scenery.Collision;
using BabelRush.Scenery.Stages.Template;

namespace BabelRush.Scenery.Stages;

public sealed class Stage(StageTemplate template, StageNode startNode) : IDisposable
{
    #region Dispose

    public void Dispose()
    {
        if (Disposed) return;

        Disposed = true;
        _dispose?.Invoke();
    }

    public bool Disposed { get; private set; }

    private Action? _dispose;

    #endregion


    public StageTemplate Template => template;
    public StageNode StartNode => startNode;

    internal Scene CreateScene()
    {
        ObjectDisposedException.ThrowIf(Disposed, this);

        Area? junctionArea = null;
        var result = new Scene();

        AddRooms(StartNode);

        var unReg = Game.GameEventBus.SubscribeAsync<ObjectEnteredAreaEvent>(async e =>
        {
            if (result.Disposed) return;
            if (e.Object != Game.Play!.BattleField.Player || e.Area != junctionArea) return;

            Game.Play.PlayerState.WantMove = false;
            StageNode nextNode = await Task.Run(StageNode () => null!); // todo: await player for their decision
            throw new NotImplementedException();

            Game.Play.PlayerState.WantMove = true;
            AddRooms(nextNode);
        });
        _dispose += unReg;

        return result;

        void AddRooms(StageNode node)
        {
            while (node.NextRooms is [var n])
            {
                result.AddRoom(node.Room.CreateRoom(), true);
                node = n;
            }
            var finalRoom = node.Room.CreateRoom();
            result.AddRoom(finalRoom, true);

            if (node.NextRooms is []) return;
            junctionArea = null!; // todo: create new junction area with marker
            throw new NotImplementedException();
            result.CollisionSpace.AddArea(junctionArea);
        }
    }


    public static Stage Default { get; } = new(StageTemplate.Default, StageNode.Default);
}