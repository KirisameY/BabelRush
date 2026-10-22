using BabelRushR.Core.Entity;
using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.Battle;

/// <summary>
/// 战斗相关事件的基类型。
/// </summary>
public abstract record BattleEvent : GamePlayEvent;

/// <summary>
/// 第一个敌人加入、<see cref="IBattle.IsInBattle"/> 已翻为 <c>true</c> 之后发布。
/// </summary>
/// <remarks>
/// 事件处理器看到的阵营列表与 <see cref="IBattle.IsInBattle"/> 都已是落定后的值。
/// </remarks>
public sealed record BattleStartedEvent : BattleEvent;

/// <summary>
/// 最后一个敌人离开、<see cref="IBattle.IsInBattle"/> 已翻为 <c>false</c> 之后发布。
/// </summary>
/// <remarks>
/// 它只描述战斗状态的翻转；我方成员可以在战斗之外继续加入/离开，那些变化仍会发布
/// <see cref="BattleUnitJoinedEvent"/> / <see cref="BattleUnitLeftEvent"/>，但不影响
/// <see cref="IBattle.IsInBattle"/>。
/// </remarks>
public sealed record BattleEndedEvent : BattleEvent;

/// <summary>
/// 单位已加入 <paramref name="Side"/> 阵营之后发布。
/// </summary>
public sealed record BattleUnitJoinedEvent(IEntity Unit, BattleSide Side) : BattleEvent;

/// <summary>
/// 单位已离开 <paramref name="Side"/> 阵营之后发布。
/// </summary>
public sealed record BattleUnitLeftEvent(IEntity Unit, BattleSide Side) : BattleEvent;
