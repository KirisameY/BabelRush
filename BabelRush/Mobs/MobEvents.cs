using KirisameLib.Events;

namespace BabelRush.Mobs;

//Base
public record MobEvent(Mob Mob) : BaseEvent;

//State
public record MobHealthChangedEvent(Mob Mob, int OldValue, int NewValue) : MobEvent(Mob);

public record MobMaxHealthChangedEvent(Mob Mob, int OldValue, int NewValue) : MobEvent(Mob);

//Interact
public record MobSelectedEvent(Mob Mob, bool ByCursor, bool Selected) : MobEvent(Mob);