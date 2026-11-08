using System.Runtime.CompilerServices;

using BabelRushR.Core.Common;
using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.Battle;

public sealed class Battle : ObservableObject
{
    private readonly IGamePlay _gamePlay;
    private readonly NotifiableList<IEntity> _allies = [];
    private readonly NotifiableList<IEntity> _enemies = [];
    private readonly Dictionary<IEntity, BattleSide> _sides = [];

    public Battle(IGamePlay gamePlay)
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

public static class BattleAttachment
{
    private static readonly ConditionalWeakTable<IGamePlay, Battle> _cache = [];

    extension(IGamePlay gamePlay)
    {
        public Battle Battle => _cache.GetOrAdd(gamePlay, play => new(play));
    }
}
