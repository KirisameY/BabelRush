using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BabelRush.Logging;

public partial class Logger
{
    private sealed class LogWriter : IDisposable
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
            _task = Task.Run(() => WriteLogAsync(_cancellationTokenSource.Token)).ContinueWith(_ =>
            {
                _logFile.Close();
            }, continuationOptions: TaskContinuationOptions.ExecuteSynchronously);
        }

        private async Task WriteLogAsync(CancellationToken cancellationToken)
        {
            // ReSharper disable MethodHasAsyncOverloadWithCancellation
            // ReSharper disable MethodHasAsyncOverload
            var writer = new StreamWriter(_logFile);
            while (!cancellationToken.IsCancellationRequested)
            {
                while (_logQueue.TryDequeue(out var log))
                {
                    writer.WriteLine(log);
                }

                writer.Flush();
                try { await Task.Delay(8000, cancellationToken); }
                catch (TaskCanceledException) { }
            }

            while (_logQueue.TryDequeue(out var log))
            {
                writer.WriteLine(log);
            }

            writer.Flush();
            writer.Close();
            // ReSharper restore MethodHasAsyncOverload
            // ReSharper restore MethodHasAsyncOverloadWithCancellation
        }

        private static FileStream InitLogFile()
        {
            DirectoryInfo logDir = new(Project.LogDirPath);
            if (!logDir.Exists) logDir.Create();
            var logFiles =
                logDir.EnumerateFiles()
                      .Where(file => file.Name.Contains(".log"))
                      .OrderByDescending(log => log.Name)
                      .ToList();
            for (int i = logFiles.Count; i > Project.MaxLogFileCount - 1; i--)
            {
                logFiles[i - 1].Delete();
            }

            var filePath = $"{Project.LogDirPath}/{Project.Name}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.log";
            var logFile = File.Open(filePath, FileMode.OpenOrCreate);
            return logFile;
        }

        public async void Dispose()
        {
            await _cancellationTokenSource.CancelAsync();
            await _task;
        }
    }
}