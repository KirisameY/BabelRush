using System.Collections.Concurrent;

namespace KirisameLib.Logging;

public static partial class LogManager
{
    private static ConcurrentQueue<Log> LogQueue { get; } = [];
    private static LogWriter? Writer { get; set; }

    private static Action<string>? RuntimePrinter { get; set; }

    private static bool Initialized { get; set; } = false;

    public static void Initialize(Action<string> runtimePrinter, string logDirPath, string logFileName, int maxLogFileCount)
    {
        if (Initialized)
        {
            Log(new(LogLevel.Warning, nameof(LogManager), "Request for duplicate initialization of Logger"));
        }

        Writer = new(LogQueue, logDirPath, logFileName, maxLogFileCount);
        RuntimePrinter = runtimePrinter;
        Initialized = true;
    }

    public static void Dispose()
    {
        Writer?.Dispose();
    }

    //Logging
    private static Dictionary<string, Logger> Loggers { get; } = [];

    public static Logger GetLogger(string source)
    {
        if (!Loggers.TryGetValue(source, out Logger? logger))
        {
            logger = new Logger(source, Log);
            Loggers.Add(source, logger);
        }

        return logger;
    }

    private static void Log(Log log)
    {
        RuntimePrinter?.Invoke(log.ToString());
        LogQueue.Enqueue(log);
    }
}