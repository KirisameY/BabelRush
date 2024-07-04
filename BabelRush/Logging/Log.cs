namespace BabelRush.Logging;

public record Log(LogLevel Level, string Message)
{
    public override string ToString() =>
        $"[{Level}] {Message}";
}

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal,
}