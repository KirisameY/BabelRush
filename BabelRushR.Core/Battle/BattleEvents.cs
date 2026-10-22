using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.Battle;

/// <summary>
/// 战斗相关事件的基类型。
/// </summary>
public abstract record BattleEvent : GamePlayEvent;

/// <summary>
/// 战斗已开始。
/// </summary>
public sealed record BattleStartedEvent : BattleEvent;

/// <summary>
/// 战斗已结束，意味着最后一个敌人已经离开战斗。
/// </summary>
public sealed record BattleEndedEvent : BattleEvent;

/// <summary>
/// 单位已加入 <paramref name="Side"/> 阵营。
/// </summary>
public sealed record BattleUnitJoinedEvent(IEntity Unit, BattleSide Side) : BattleEvent;

/// <summary>
/// 单位已离开 <paramref name="Side"/> 阵营。
/// </summary>
public sealed record BattleUnitLeftEvent(IEntity Unit, BattleSide Side) : BattleEvent;
