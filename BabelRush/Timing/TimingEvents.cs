using KirisameLib.Event;

namespace BabelRush.Timing;

public abstract record TimingEvent : BaseEvent;

public sealed record TimeScaleChangedEvent(double PrevValue, double NewValue) : TimingEvent;