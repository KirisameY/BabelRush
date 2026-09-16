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
    /// <para>
    /// 这里是静态工厂而非重载构造函数：主构造函数要求其余构造函数必须链到它，
    /// 而这三件套需要共享同一个总线实例，初始化器里无法做到。
    /// </para>
    /// </summary>
    public static CommonGamePlay Create(IEntity pcEntity, int maxAP = 3, double apRegeneration = 1.0)
    {
        var eventBus = new AutoEventBus();
        var gamePlay = new CommonGamePlay(
            new CommonScene(eventBus),
            new CommonPlayerState(pcEntity, eventBus, maxAP, apRegeneration),
            eventBus);

        // AddEntity 只返回 Order，不提交就不会有人收到 EntityAddedEvent。
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

        DeltaTime = delta;
        Time += delta;

        // 先结算费用，再推进实体：实体的 OnUpdate 触发效果时本帧费用已经就绪。
        // 两者各自接管自己那部分的事件分发。
        PlayerState.Update(delta);
        Scene.Update(delta);
    }
}
