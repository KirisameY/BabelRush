using System.Collections.Concurrent;

using JetBrains.Annotations;

namespace KirisameLib.Logging;

[MustDisposeResource]
public sealed partial class Logger : IDisposable
{
    private readonly ConcurrentQueue<Log> _logQueue = [];
    private readonly LogWriter _writer;

    private readonly Action<string> _runtimePrinter;

    public Logger(Action<string> runtimePrinter, string logDirPath, string logFileName, int maxLogFileCount)
    {
        _writer = new(_logQueue, logDirPath, logFileName, maxLogFileCount);
        _runtimePrinter = runtimePrinter;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    //Operation
    public void Log(Log log)
    {
        _runtimePrinter(log.ToString());
        _logQueue.Enqueue(log);
    }
}