using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Godot;

using JetBrains.Annotations;

namespace BabelRush.Logging;

[MustDisposeResource]
public sealed class Logger : IDisposable
{
    private readonly ConcurrentQueue<Log> _logQueue = [];
    private readonly LogWriter _writer;

    public Logger()
    {
        _writer = new(_logQueue);
    }

    public void Log(Log log)
    {
        GD.Print(log);
        _logQueue.Enqueue(log);
    }

    public void Dispose()
    {
        _writer.Dispose();
    }


    private class LogWriter : IDisposable
    {
        private readonly ConcurrentQueue<Log> _logQueue;
        private readonly FileStream _logFile;
        private readonly CancellationTokenSource _cancellationTokenSource;
        private readonly Task _task;

        public LogWriter(ConcurrentQueue<Log> logQueue)
        {
            _logQueue = logQueue;
            _logFile = InitLogFile();
            _cancellationTokenSource = new();
            _task = Task.Run(() => WriteLogAsync(_cancellationTokenSource.Token));
        }

        private void WriteLogAsync(CancellationToken cancellationToken)
        {
            var writer = new StreamWriter(_logFile);
            while (!cancellationToken.IsCancellationRequested)
            {
                while (_logQueue.TryDequeue(out var log))
                {
                    writer.WriteLine(log);
                }

                writer.Flush();
                Task.Delay(8000, cancellationToken).Wait(CancellationToken.None);
            }

            while (_logQueue.TryDequeue(out var log))
            {
                writer.Write($"{log}\n");
            }

            writer.Flush();
            writer.Close();
        }

        private static FileStream InitLogFile()
        {
            DirectoryInfo logDir = new(Project.LogDirPath);
            if (!logDir.Exists) logDir.Create();
            var logFiles = logDir.EnumerateFiles().OrderByDescending(log => log.Name).ToList();
            for (int i = logFiles.Count; i > Project.MaxLogFileCount; i--)
            {
                logFiles[i - 1].Delete();
            }

            var filePath = $"{Project.LogDirPath}/{Project.Name}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.log";
            var logFile = File.Open(filePath, FileMode.OpenOrCreate);
            return logFile;
        }

        public void Dispose()
        {
            _cancellationTokenSource.Cancel();
            _task.Wait();
            _logFile.Close();
        }
    }
}