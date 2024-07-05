using System;
using System.Collections.Concurrent;

using Godot;

using JetBrains.Annotations;

namespace BabelRush.Logging;

[MustDisposeResource]
public sealed partial class Logger : IDisposable
{
    private readonly ConcurrentQueue<Log> _logQueue = [];
    private readonly LogWriter _writer;

    public Logger()
    {
        _writer = new(_logQueue);
    }

    public void Dispose()
    {
        _writer.Dispose();
    }
    
    //Operation
    public void Log(Log log)
    {
        GD.Print(log);
        _logQueue.Enqueue(log);
    }
}