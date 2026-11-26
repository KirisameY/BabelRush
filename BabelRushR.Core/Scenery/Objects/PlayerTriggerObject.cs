using System.Runtime.CompilerServices;

using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.Scenery.Objects;

public abstract class PlayerTriggerObject(double range) : SceneObjectBase, IUpdatable
{
    private IEntity? _pc;

    private static readonly ConditionalWeakTable<IScene, List<PlayerTriggerObject>> _unattached = [];

    protected sealed override void Attached(IScene to)
    {
        if (to.GamePlay is not { } gamePlay) { }
        else if (gamePlay.PlayerState.PCEntity is { } pc)
        {
            _pc = pc;
        }
    }

    protected sealed override void Unattached(IScene from)
    {
        _pc = null;
    }

    public void Update(double delta)
    {
        if (_pc is null) return;
        if (_pc.Scene != Scene) return;
        var pos = _pc.Position;
        if (Math.Abs(pos - Position) <= range) Triggered(_pc);
    }

    protected abstract void Triggered(IEntity pc);


    [AutoGamePlayEventHandler]
    private class EventBusPreAttacher : IGamePlayAttachedLogic
    {
        public static IDisposable? Register(IGamePlay gamePlay)
        {
            gamePlay.EventBus.Subscribe<GamePlaySceneReplacedEvent>(e =>
            {
                var newScene = e.NewScene;
                if (!_unattached.Remove(newScene, out var triggers)) return;
                var pcEntity = gamePlay.PlayerState.PCEntity;
                foreach (var trigger in triggers)
                {
                    trigger._pc = pcEntity;
                }
            });
            return null;
        }
    }
}