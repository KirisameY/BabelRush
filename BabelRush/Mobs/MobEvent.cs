using KirisameLib.Events;

namespace BabelRush.Mobs;

public record MobEvent(Mob Mob) : BaseEvent;

public record MobHealthChangedEvent(Mob Mob, int OldValue, int NewValue) : MobEvent(Mob);

public record MobMaxHealthChangedEvent(Mob Mob, int OldValue, int NewValue) : MobEvent(Mob);