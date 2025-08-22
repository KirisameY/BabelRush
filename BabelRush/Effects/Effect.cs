using System;
using System.Threading.Tasks;

using BabelRush.Mobs;

namespace BabelRush.Effects;

public abstract class Effect(EffectType type, int value = 0)
{
    public EffectType Type => type;

    public int Value { get; private set; } = value;

    public Mob? AffectedMob { get; private set; }

    public double TotalTime { get; private set; }
    public double RemainTime { get; private set; }


    internal async Task<double?> ApplyTo(Mob mob, double time)
    {
        if (AffectedMob is not null) throw new InvalidOperationException("Tried to apply an effect that was already applied to some mob");

        var request = await Game.GameEventBus.PublishAndWaitFor(new EffectApplyRequestEvent(this, mob, time, new()));
        if (request.Cancel.Canceled) return null;
        time = request.Time;

        AffectedMob = mob;
        TotalTime   = RemainTime = time;
        Applied();

        Game.GameEventBus.Publish(new EffectAppliedEvent(this, mob, time));
        return time;
    }

    internal bool ProcessUpdate(double delta)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to execute process update on an effect that didn't apply to any mob");

        RemainTime = Math.Max(0, RemainTime - delta);
        Process(delta);

        return RemainTime <= 0;
    }

    internal async Task<bool> Remove(bool natural)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to remove an effect that didn't apply to any mob");

        if (!natural &&
            (await Game.GameEventBus.PublishAndWaitFor(new EffectRemoveRequestEvent(this, new()))).Cancel.Canceled)
            return false;

        if (!BeforeRemoved() && !natural) return false;

        await Game.GameEventBus.PublishAndWaitFor(new EffectRemovedEvent(this));
        AffectedMob = null;
        return true;
    }

    public async Task<int?> UpdateValue(int newValue)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to update time on an effect that didn't apply to any mob");
        if (newValue == Value) return null;

        var request = await Game.GameEventBus.PublishAndWaitFor(new EffectValueUpdateRequestEvent(this, newValue, new()));
        if (request.Cancel.Canceled) return null;
        newValue = request.NewValue;
        if (newValue == Value) return null;

        if (!BeforeValueUpdated(ref newValue) || newValue == Value) return null;

        (var prevValue, Value) = (Value, newValue);
        Game.GameEventBus.Publish(new EffectValueUpdatedEvent(this, prevValue, newValue));
        return newValue;
    }

    public async Task<double?> UpdateTime(double newTime)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to update time on an effect that didn't apply to any mob");

        var request = await Game.GameEventBus.PublishAndWaitFor(new EffectTimeUpdateRequestEvent(this, newTime, new()));
        if (request.Cancel.Canceled) return null;
        newTime = request.NewTime;

        if (BeforeTimeUpdated(ref newTime)) return null;

        var prev = (TotalTime, RemainTime);
        if (newTime > TotalTime) TotalTime = newTime;
        Game.GameEventBus.Publish(new EffectTimeUpdatedEvent(this, prev.TotalTime, prev.RemainTime, newTime));
        return newTime;
    }

    protected abstract void Applied();
    protected abstract void Process(double delta);
    protected abstract bool BeforeRemoved();

    protected virtual bool BeforeValueUpdated(ref int newValue) => true;
    protected virtual bool BeforeTimeUpdated(ref double newTime) => true;
}