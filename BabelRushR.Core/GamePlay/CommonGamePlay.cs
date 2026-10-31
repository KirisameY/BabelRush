using BabelRushR.Core.Battle;
using BabelRushR.Core.Card.Operations;
using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.Scenery;

using KirisameY.EventBus;
using KirisameY.EventBus.Bus;
using KirisameY.Randomizer;

namespace BabelRushR.Core.GamePlay;

/// <summary>
///     <see cref="IGamePlay"/> 的默认实现。
/// </summary>
/// <remarks>
///     通过 <see cref="Create"/> 创建时会自动注册 <see cref="IGamePlayEventHandler"/>；
///     手动装配（对象初始化器）后需要自行调用一次 <see cref="InitializeEventHandlers"/>。
/// </remarks>
public class CommonGamePlay(ulong seed) : ObservableObject, IGamePlay
{
    private IDisposable? _eventHandlerRegistrations;
    private bool _eventHandlersInitialized;
    private bool _disposed;

    /// <summary>
    /// 自建事件总线、场景与玩家状态，放入玩家实体，并注册自动发现的事件处理器。
    /// </summary>
    public static CommonGamePlay Create(ulong seed, IEntity pcEntity, int maxAP = 6, double apRegeneration = 1.0)
    {
        var gamePlay = new CommonGamePlay(seed)
        {
            Scene       = new CommonScene(),
            PlayerState = new CommonPlayerState(pcEntity, maxAP, apRegeneration),
        };

        gamePlay.Scene.AddEntity(pcEntity);
        gamePlay.InitializeEventHandlers();
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

    /// <summary>
    /// 自动发现并注册本局的事件处理器。
    /// </summary>
    /// <remarks>
    ///     <see cref="Create"/> 会自动调用；手动装配的实例需要自行调用一次。本方法幂等，
    ///     且在释放之后调用不会做任何事。注册失败时不会重试：已经建立的注册会被撤销，异常会继续抛出。
    /// </remarks>
    public void InitializeEventHandlers()
    {
        if (_eventHandlersInitialized || _disposed) return;
        _eventHandlersInitialized = true;

        var registrations = GamePlayEventHandlerRegistry.RegisterAll(this);
        // 处理器可能在注册过程中释放了本局；此时刚建立的注册也不能留下。
        if (_disposed)
        {
            registrations.Dispose();
            return;
        }
        _eventHandlerRegistrations = registrations;
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        Interlocked.Exchange(ref _eventHandlerRegistrations, null)?.Dispose();
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