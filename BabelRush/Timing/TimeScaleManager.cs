namespace BabelRush.Timing;

internal static class TimeScaleManager
{
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
        Game.GameEventBus.Publish(new TimeScaleChangedEvent(prev, @new));
    }


    internal static void ProcessUpdate(double delta)
    {
        var scale = TimeScale;
    }
}