using KirisameLib.Event;

namespace BabelRush.Mobs;

//Base
public abstract record MobEvent(Mob Mob) : BaseEvent;

//State
public sealed record MobHealthChangedEvent(Mob Mob, int OldValue, int NewValue) : MobEvent(Mob);

public sealed record MobMaxHealthChangedEvent(Mob Mob, int OldValue, int NewValue) : MobEvent(Mob);

public sealed record MobAlignmentChangedEvent(Mob Mob, Alignment OldAlignment, Alignment NewAlignment) : MobEvent(Mob);

//Interact
public sealed record MobSelectedEvent(Mob Mob, bool ByCursor, bool Selected) : MobEvent(Mob);