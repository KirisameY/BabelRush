using KirisameLib.Event;

namespace BabelRush.Mobs.Actions;

public abstract record MobActionEvent(Mob Mob, MobAction Action) : MobEvent(Mob);

public sealed record MobActionStartedEvent(Mob Mob, MobAction Action) : MobActionEvent(Mob, Action);

public sealed record MobActionExecutedEvent(Mob Mob, MobAction Action) : MobActionEvent(Mob, Action);

public sealed record MobActionCanceledEvent(Mob Mob, MobAction Action) : MobActionEvent(Mob, Action);

public sealed record MobActionInterruptedEvent(Mob Mob, MobAction Action) : MobActionEvent(Mob, Action);

public sealed record MobActionExecuteRequestEvent(Mob Mob, MobAction Action, CancelToken Cancel) : MobActionEvent(Mob, Action);