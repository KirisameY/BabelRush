using System.Runtime.CompilerServices;

using BabelRushR.Core.Battle;
using BabelRushR.Core.Card.Operations;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Level;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus;
using KirisameY.EventBus.Bus;
using KirisameY.Randomizer;

namespace BabelRushR.Core.GamePlay;

/// <summary>
///     <see cref="IGamePlay"/> 的默认实现。
/// </summary>
/// <remarks>
///     通过 <see cref="Create"/> 创建时会自动注册 <see cref="IGamePlayAttachedLogic"/>；
///     手动装配（对象初始化器）后需要自行调用一次 <see cref="InitializeEventHandlers"/>。
/// </remarks>
public class CommonGamePlay(ulong seed) : ObservableObject, IGamePlay
{
    /// <summary>
    /// 自建事件总线、场景、关卡与玩家状态，放入玩家实体，并注册自动发现的事件处理器。
    /// </summary>
    public static CommonGamePlay Create(ulong seed, IEntity pcEntity, ILevel? level = null, int maxAP = 6, double apRegeneration = 1.0)
    {
        var gamePlay = new CommonGamePlay(seed)
        {
            Scene       = new CommonScene(),
            PlayerState = new CommonPlayerState(pcEntity, maxAP, apRegeneration),
            Level       = level ?? new CommonLevel(),
        };

        gamePlay.Scene.AddEntity(pcEntity);
        gamePlay.Battle.TryJoinBattle(pcEntity, BattleSide.Ally);
        gamePlay.Level.TryBegin();
        GamePlayAttachedLogicManager.RegisterDiscovered(gamePlay);
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

    // todo: 一般來說 Level 需要從關卡生成器生成，而後者需要請求隨機器，而隨機器需要先創建實例。故不應該將level設爲required。
    //       也許我們是需要先將 Scene 置空 Level 佔位，裝配 GamePlay 時傳入第一個關卡的生成器，然後生成後設定 Level。
    public required ILevel Level
    {
        get;
        set
        {
            var old = field;
            if (!SetProperty(ref field, value)) return;

            old?.Unattach();
            value.Attach(this);
        }
    }

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

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void InitializeEventHandlers() => GamePlayAttachedLogicManager.RegisterDiscovered(this);

    private bool _disposed;
    public bool Disposed => _disposed;

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, true)) return;
        Disposing?.Invoke();
    }

    public event Action? Disposing;
}