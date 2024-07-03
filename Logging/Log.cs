namespace BabelRush.Logging;

public record class Log(LogLevel Level, string Message);

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error,
    Fatal,
}