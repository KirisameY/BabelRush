using System;

using BabelRush.Mobs;
using BabelRush.Utils;

using KirisameLib.Asynchronous.SyncTasking;
using KirisameLib.Logging;

namespace BabelRush.Effects;

public abstract partial class Effect(EffectType type, int value = 0)
{
    public EffectType Type => type;

    public int Value { get; private set; } = value;

    public Mob? AffectedMob { get; private set; }

    public double TotalTime { get; private set; }
    public double RemainTime { get; private set; }


    [LogToSync]
    internal async SyncTask<double?> ApplyToAsync(Mob mob, double time)
    {
        if (AffectedMob is not null) throw new InvalidOperationException("Tried to apply an effect that was already applied to some mob");

        var request = await Game.GameEventBus.PublishAndWaitFor(new EffectApplyRequestEvent(this, mob, time, new()));
        if (request.Cancel.Canceled) return null;
        time = request.Time;

        AffectedMob = mob;
        TotalTime   = RemainTime = time;
        Applied();

        await Game.GameEventBus.PublishAndWaitFor(new EffectAppliedEvent(this, mob, time));
        return time;
    }

    internal bool ProcessUpdate(double delta)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to execute process update on an effect that didn't apply to any mob");

        RemainTime = Math.Max(0, RemainTime - delta);
        Process(delta);

        return RemainTime <= 0;
    }

    [LogToSync]
    internal async SyncTask<bool> RemoveAsync(bool natural)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to remove an effect that didn't apply to any mob");

        if (!natural &&
            (await Game.GameEventBus.PublishAndWaitFor(new EffectRemoveRequestEvent(this, new()))).Cancel.Canceled)
            return false;

        if (!BeforeRemoved()) return false;

        await Game.GameEventBus.PublishAndWaitFor(new EffectRemovedEvent(this));
        AffectedMob = null;
        return true;
    }

    // public void UpdateValue(int newValue) => UpdateValueAsync(newValue).ContinueWith(t =>
    // {
    //     if (!t.IsFaulted) return;
    //     Logger.Log(LogLevel.Error, nameof(UpdateValue), $"Exception thrown: {t.Exception?.Flatten()}");
    //     Logger.Log(LogLevel.Debug, nameof(UpdateValue), $"StackTrace: {t.Exception?.StackTrace}");
    // });

    [LogToSync]
    public async SyncTask<int?> UpdateValueAsync(int newValue)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to update value on an effect that didn't apply to any mob");
        if (newValue == Value) return null;

        var request = await Game.GameEventBus.PublishAndWaitFor(new EffectValueUpdateRequestEvent(this, newValue, new()));
        if (request.Cancel.Canceled) return null;
        newValue = request.NewValue;
        if (newValue == Value) return null;

        if (!BeforeValueUpdated(ref newValue) || newValue == Value) return null;

        (var prevValue, Value) = (Value, newValue);
        await Game.GameEventBus.PublishAndWaitFor(new EffectValueUpdatedEvent(this, prevValue, newValue));
        return newValue;
    }

    // public void UpdateTime(double newTime) => UpdateTimeAsync(newTime).ContinueWith(t =>
    // {
    //     if (!t.IsFaulted) return;
    //     Logger.Log(LogLevel.Error, nameof(UpdateTime), $"Exception thrown: {t.Exception?.Flatten()}");
    //     Logger.Log(LogLevel.Debug, nameof(UpdateTime), $"StackTrace: {t.Exception?.StackTrace}");
    // });

    [LogToSync]
    public async SyncTask<double?> UpdateTimeAsync(double newTime)
    {
        if (AffectedMob is null) throw new InvalidOperationException("Tried to update time on an effect that didn't apply to any mob");

        var request = await Game.GameEventBus.PublishAndWaitFor(new EffectTimeUpdateRequestEvent(this, newTime, new()));
        if (request.Cancel.Canceled) return null;
        newTime = request.NewTime;

        if (!BeforeTimeUpdated(ref newTime)) return null;

        var prev = (TotalTime, RemainTime);
        if (newTime > TotalTime) TotalTime = newTime;
        RemainTime = newTime;
        Game.GameEventBus.Publish(new EffectTimeUpdatedEvent(this, prev.TotalTime, prev.RemainTime, newTime));
        return newTime;
    }

    protected abstract void Applied();
    protected abstract void Process(double delta);
    protected abstract bool BeforeRemoved();

    protected virtual bool BeforeValueUpdated(ref int newValue) => true;
    protected virtual bool BeforeTimeUpdated(ref double newTime) => true;


    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(Effect));
}