using BabelRush.Mobs;

using KirisameLib.Events;

namespace BabelRush.GamePlay;

//Base
public abstract record GamePlayEvent : BaseEvent;

//Events
public abstract record MobListChangedEvent : GamePlayEvent;

public sealed record AlignmentUpdatedEvent : MobListChangedEvent;

public sealed record MobAddedEvent(Mob Mob) : MobListChangedEvent;

public sealed record MobRemovedEvent(Mob Mob) : MobListChangedEvent;

//Battle
public abstract record BattleEvent : GamePlayEvent;

public sealed record BattleStartEvent : BattleEvent;

public sealed record BattleEndEvent : BattleEvent;