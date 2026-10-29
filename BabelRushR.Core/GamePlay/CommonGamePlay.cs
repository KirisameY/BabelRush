using BabelRushR.Core.Battle;
using BabelRushR.Core.Card.Operations;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus;
using KirisameY.EventBus.Bus;
using KirisameY.Randomizer;

namespace BabelRushR.Core.GamePlay;

public class CommonGamePlay(ulong seed) : ObservableObject, IGamePlay
{
    /// <summary>
    /// 自建事件总线、场景与玩家状态，并把玩家实体放入场景。
    /// </summary>
    public static CommonGamePlay Create(ulong seed, IEntity pcEntity, int maxAP = 6, double apRegeneration = 1.0)
    {
        var gamePlay = new CommonGamePlay(seed)
        {
            Scene       = new CommonScene(),
            PlayerState = new CommonPlayerState(pcEntity, maxAP, apRegeneration),
        };

        gamePlay.Scene.AddEntity(pcEntity);
        return gamePlay;
    }

    public IEventBus<GamePlayEvent> EventBus => field ??= new SimpleEventBus<GamePlayEvent>();


    private readonly GameRandoms _randoms = new(seed);
    public ulong RandomSeed => _randoms.Seed;
    public IRandomizer GetRandomizer(string scopeName) => _randoms.GetRandomizer(scopeName);


    public required IScene Scene
    {
        get;
        set
        {
            var old = field;
            if (!SetProperty(ref field, value)) return;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            old?.Unattach();
            value.Attach(this);
            if (old is not null) EventBus.Publish(new GamePlaySceneReplacedEvent(old, value));
        }
    }

    public required IPlayerState PlayerState
    {
        get;
        init
        {
            field = value;
            value.Initialize(this);
        }
    }

    public ICardDirector CardDirector => field ??= new CommonCardDirector(this);

    public IBattle Battle => field ??= new CommonBattle(this);

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