using KirisameLib.Events;
using KirisameLib.Logging;

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
            Logger.Log(LogLevel.Info, "Setting", $"Local changed to '{value}' from '{prev}'");
            EventBus.Publish(new LocalChangedEvent(prev, value));
        }
    }

    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(LocalSettings));
}