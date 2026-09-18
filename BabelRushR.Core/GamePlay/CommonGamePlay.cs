using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus;
using KirisameY.EventBus.Bus;

namespace BabelRushR.Core.GamePlay;

public class CommonGamePlay(IScene scene, IPlayerState playerState, IEventBus eventBus) : ObservableObject, IGamePlay
{
    /// <summary>
    /// 自建事件总线、场景与玩家状态，并把玩家实体放入场景。
    /// </summary>
    public static CommonGamePlay Create(IEntity pcEntity, int maxAP = 3, double apRegeneration = 1.0)
    {
        var eventBus = new AutoEventBus();
        var gamePlay = new CommonGamePlay(
            new CommonScene(eventBus),
            new CommonPlayerState(pcEntity, eventBus, maxAP, apRegeneration),
            eventBus);

        gamePlay.Scene.AddEntity(pcEntity).Submit();
        return gamePlay;
    }

    public IEventBus EventBus => eventBus;

    public IScene Scene => scene;

    public IPlayerState PlayerState => playerState;

    public double Time
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public double DeltaTime
    {
        get;
        private set => SetProperty(ref field, value);
    }

    public void Update(double delta)
    {
        if (!double.IsFinite(delta) || delta <= 0) return;

        DeltaTime =  delta;
        Time      += delta;

        PlayerState.Update(delta);
        Scene.Update(delta);
    }
}