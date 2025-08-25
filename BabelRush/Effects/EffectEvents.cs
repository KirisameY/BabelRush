using BabelRush.Mobs;

using KirisameLib.Event;

namespace BabelRush.Effects;

public abstract record EffectEvent(Effect Effect) : BaseEvent;

public sealed record EffectApplyRequestEvent(Effect Effect, Mob Target, Variable<double> Time, CancelToken Cancel) : EffectEvent(Effect);

public sealed record EffectAppliedEvent(Effect Effect, Mob Target, double Time) : EffectEvent(Effect);

public sealed record EffectRemoveRequestEvent(Effect Effect, CancelToken Cancel) : EffectEvent(Effect);

public sealed record EffectRemovedEvent(Effect Effect) : EffectEvent(Effect);

public sealed record EffectValueUpdateRequestEvent(Effect Effect, Variable<int> NewValue, CancelToken Cancel) : EffectEvent(Effect);

public sealed record EffectValueUpdatedEvent(Effect Effect, int PrevValue, int NewValue) : EffectEvent(Effect);

public sealed record EffectTimeUpdateRequestEvent(Effect Effect, Variable<double> NewTime, CancelToken Cancel) : EffectEvent(Effect);

public sealed record EffectTimeUpdatedEvent(Effect Effect, double PrevTotalTime, double PrevRemainTime, double NewTime) : EffectEvent(Effect);