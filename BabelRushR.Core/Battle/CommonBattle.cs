using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Battle;

/// <summary>
///     <see cref="IBattle"/> 的默认实现：两个可通知列表保存双方成员，成员变化驱动战斗状态。
/// </summary>
/// <remarks>
///     成员表直接用实体本身作字典键，依赖引用相等（与 Mvvm 的集合包装对源集合的要求一致）。<br/>
///     单位离开场景时自动退出：死亡清扫发的是 <see cref="EntityDiedEvent"/>，它派生自
///     <see cref="EntityRemovedEvent"/>，所以这里只订阅后者。<br/>
///     场景整体替换时旧场景的成员不会再收到单独的移除事件，故另外订阅
///     <see cref="GamePlaySceneReplacedEvent"/> 做全量驱逐。<br/>
///     订阅与一局游戏同寿（<see cref="IGamePlay"/> 没有释放时机），因此不做撤销。
/// </remarks>
public class CommonBattle : ObservableObject, IBattle
{
    private readonly IGamePlay _gamePlay;
    private readonly NotifiableList<IEntity> _allies = [];
    private readonly NotifiableList<IEntity> _enemies = [];
    private readonly Dictionary<IEntity, BattleSide> _sides = [];

    public CommonBattle(IGamePlay gamePlay)
    {
        _gamePlay = gamePlay;
        gamePlay.EventBus.Subscribe<EntityRemovedEvent>(OnEntityRemoved);
        gamePlay.EventBus.Subscribe<GamePlaySceneReplacedEvent>(OnSceneReplaced);
    }

    public IReadOnlyNotifiableList<IEntity> Allies => field ??= _allies.AsReadOnlyNotifiableList();

    public IReadOnlyNotifiableList<IEntity> Enemies => field ??= _enemies.AsReadOnlyNotifiableList();

    // 与 EntityBase.IsAlive 同样的写法：派生值，只在真正翻转时补一次属性通知。
    public bool IsInBattle => _enemies.Count > 0;

    public BattleSide GetSideOf(IEntity unit) => _sides.GetValueOrDefault(unit, BattleSide.None);

    public IReadOnlyList<IEntity>? GetSideEntities(BattleSide side) => side switch
    {
        BattleSide.Ally  => Allies,
        BattleSide.Enemy => Enemies,
        _                => null,
    };

    public bool TryJoinBattle(IEntity unit, BattleSide side)
    {
        if (side is BattleSide.None) return false;
        // 同一个实体同时只能属于一方；换边要先离开再加入。
        if (_sides.ContainsKey(unit)) return false;
        // 参战单位必须在当前的场景里，否则「离开场景自动退出」会留下对不上的状态。
        if (!_gamePlay.Scene.Entities.Contains(unit)) return false;

        UnitsOf(side).Add(unit);
        _sides.Add(unit, side);

        var started = side is BattleSide.Enemy && _enemies.Count == 1;
        if (started) OnPropertyChanged(nameof(IsInBattle));

        // 先报成员变化，再报状态翻转；两者都已落定，这里只是因果顺序。
        _gamePlay.EventBus.Publish(new BattleUnitJoinedEvent(unit, side));
        if (started) _gamePlay.EventBus.Publish(new BattleStartedEvent());
        return true;
    }

    public bool TryLeaveBattle(IEntity unit)
    {
        if (!_sides.Remove(unit, out var side)) return false;

        UnitsOf(side).Remove(unit);

        var ended = side is BattleSide.Enemy && _enemies.Count == 0;
        if (ended) OnPropertyChanged(nameof(IsInBattle));

        _gamePlay.EventBus.Publish(new BattleUnitLeftEvent(unit, side));
        if (ended) _gamePlay.EventBus.Publish(new BattleEndedEvent());
        return true;
    }


    private NotifiableList<IEntity> UnitsOf(BattleSide side) => side switch
    {
        BattleSide.Ally  => _allies,
        BattleSide.Enemy => _enemies,
        _                => throw new ArgumentOutOfRangeException(nameof(side), side, null),
    };


    #region Event Handlers

    private void OnEntityRemoved(EntityRemovedEvent e) => TryLeaveBattle(e.Entity);

    private void OnSceneReplaced(GamePlaySceneReplacedEvent e)
    {
        // 旧场景的成员已经不在新场景里。先快照再逐个移出：移出会改列表并发事件。
        // 先我方后敌方：最后一个敌人离开时两边都已清空，BattleEndedEvent 的终态才一致。
        IEntity[] units = [.. _allies, .. _enemies];
        foreach (var unit in units) TryLeaveBattle(unit);
    }

    #endregion
}
