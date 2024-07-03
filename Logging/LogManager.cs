using System.Collections.Concurrent;

namespace BabelRush.Logging;

public static class LogManager
{
    private static readonly ConcurrentQueue<Log> LogQueue = [];
    private static readonly LogWriter Writer = new();

    public static void Log(Log log)
    {
        LogQueue.Enqueue(log);
        //Todo: 通知记录器记录日志
    }

    private class LogWriter
    {
        //Todo: 实现异步储存日志
    }
}