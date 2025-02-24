using BabelRush.Mobs;

using KirisameLib.Event;

namespace BabelRush.GamePlay;

//Base
public abstract record GamePlayEvent : BaseEvent;

//Battle
public abstract record BattleEvent : GamePlayEvent;

public sealed record BattleStartEvent : BattleEvent;

public sealed record BattleEndEvent : BattleEvent;

//InBattle Mobs
public abstract record InBattleMobListChangedEvent : BattleEvent;

public sealed record AlignmentUpdatedEvent : InBattleMobListChangedEvent;

public sealed record InBattleMobAddedEvent(Mob Mob) : InBattleMobListChangedEvent;

public sealed record InBattleMobRemovedEvent(Mob Mob) : InBattleMobListChangedEvent;

//Player
public abstract record PlayerEvent : GamePlayEvent;

public sealed record ApChangedEvent(int OldAp, int NewAp) : PlayerEvent;