using System;
using System.Collections.Frozen;
using System.Collections.Generic;

using Godot;

using KirisameLib.Extensions;

namespace BabelRush.Timing;

using ScaledObjectTuple = (HashSet<object> objects,
    Action<object, (double prev, double @new)> updateScale,
    Action<object, (double delta, double scale)> updateProcess);

internal static class TimeScaleManager
{
    #region ScaledObjects

    private static readonly FrozenDictionary<Type, ScaledObjectTuple> TimeScaledObjDict = new Dictionary<Type, ScaledObjectTuple>
    {
        [typeof(ITimeScaled)] = ([], (obj, sc) =>
        {
            var timeScaled = (ITimeScaled)obj;
            timeScaled.UpdateScale(sc.prev, sc.@new);
        }, (obj, t) =>
        {
            var timeScaled = (ITimeScaled)obj;
            timeScaled.UpdateProcess(t.delta, t.scale);
        }),

        [typeof(Tween)] = ([], (obj, sc) =>
        {
            var tween = (Tween)obj;
            tween.SetSpeedScale((float)sc.@new);
        }, (_, _) => { }),
    }.ToFrozenDictionary();

    internal static Action AddScaledObject<T>(T obj) where T : notnull
    {
        if (!TimeScaledObjDict.TryGetValue(typeof(T), out var tuple))
            throw new ArgumentOutOfRangeException(nameof(obj), $"Invalid type: {obj.GetType()}");

        tuple.objects.Add(obj);
        return () => tuple.objects.Remove(obj);
    }

    #endregion

    internal static double TimeScale
    {
        get;
        set
        {
            // ReSharper disable once CompareOfFloatsByEqualityOperator
            if (field == value) return;

            var prev = field;
            field = value;
            OnTimeScaleChanged(prev, value);
        }
    } = 1;

    private static void OnTimeScaleChanged(double prev, double @new)
    {
        foreach (var (objects, updateScale, _) in TimeScaledObjDict.Values)
        {
            objects.ForEach(obj => updateScale(obj, (prev, @new)));
        }
        Game.GameEventBus.Publish(new TimeScaleChangedEvent(prev, @new));
    }

    internal static void ProcessUpdate(double delta)
    {
        var scale = TimeScale;

        foreach (var (objects, _, updateProcess) in TimeScaledObjDict.Values)
        {
            objects.ForEach(obj => updateProcess(obj, (delta, scale)));
        }
    }
}