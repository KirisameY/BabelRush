using KirisameLib.Events;

namespace KirisameLib.I18n;

public static class LocalSettings
{
    private static string? _local;
    public static string Local
    {
        get => _local ?? "";
        set
        {
            var prev = Local;
            _local = value;
            EventBus.Publish(new LocalChangedEvent(prev, value));
        }
    }
}