using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using BabelRush.I18n;
using BabelRush.Registering;
using BabelRush.Scripting;

using Godot;

using KirisameLib.Data.I18n;
using KirisameLib.Event;
using KirisameLib.Event.Generated;
using KirisameLib.FileSys;
using KirisameLib.Logging;
using KirisameLib.Godot.IO;

namespace BabelRush;

[GlobalClass]
public partial class Game : SceneTree
{
    //Singleton
    private Game() { }

    [field: AllowNull, MaybeNull]
    public static Game Instance
    {
        get => field ?? throw new GameNotInitializedException();
        private set;
    }


    //Properties
    [field: AllowNull, MaybeNull]
    public static LogBus LogBus
    {
        get => field ?? throw new GameNotInitializedException();
        private set;
    }

    private static readonly DelayedEventBus InnerEventBus = new();
    public static EventBus EventBus => InnerEventBus;

    public static class Localization
    {
        static Localization()
        {
            LocalizedRegister.LocalChangedEvent += static (prev, current) => EventBus.Publish(new LocalChangedEvent(prev, current));
        }

        public static string Local
        {
            get => LocalizedRegister.Local;
            set => LocalizedRegister.Local = value;
        }
    }


    //Initialization
    public override void _Initialize()
    {
        Instance = this;
        LogInitialize();

        Logger.Log(LogLevel.Info, "Initializing", "Subscribing default static event handlers...");
        GlobalEventHandlersSubscriber.Subscribe(EventBus);

        base._Initialize();

        Logger.Log(LogLevel.Info, "Initializing", "Game class loaded");

        Logger.Log(LogLevel.Info, "Initializing", "Loading Script...");
        ScriptHub.Initialize();

        // 回头可以把这个拿走到单独的地方，但现在为了不叠不必要的任务栈影响开发，先这样
        Logger.Log(LogLevel.Info, "Initializing", "Loading assets...");
        LoadAssets();
        Logger.Log(LogLevel.Info, "Initializing", "Assets loading completed");
    }

    private static void LogInitialize()
    {
        DirectoryInfo logDir = new(Project.Logging.LogDirPath);
        if (!logDir.Exists) logDir.Create();
        var logFiles =
            logDir.EnumerateFiles()
                  .Where(file => file.Name.Contains(".log"))
                  .OrderByDescending(log => log.Name)
                  .ToList();
        for (int i = logFiles.Count; i > Project.Logging.MaxLogFileCount - 1; i--)
        {
            logFiles[i - 1].Delete();
        }
        var filePath = $"{Project.Logging.LogDirPath}/{Project.Name}_{DateTime.Now:yyyy-MM-dd_HH-mm-ss-fff}.log";
        var logFile = File.Open(filePath, FileMode.OpenOrCreate);

        LogBus = new WriterLogBus(Project.Logging.MinLogLevel,
                                  new StreamWriter(logFile, Encoding.UTF8),
                                  new GdConsoleWriter());
    }

    private static void LoadAssets()
    {
        BabelRushGenerated.BabelRush.RegisterMap.Register();

    #if TOOLS
        var root = new DirectoryInfo("./assets").ToReadableDirectory();
        if (!root.Exists)
        {
            Logger.Log(LogLevel.Error, "LoadingAssets", "Assets directory not found");
            return;
        }
    #else
        var zipInfo = new FileInfo("./assets.zip");
        if (!zipInfo.Exists)
        {
            Logger.Log(LogLevel.Error, "LoadingAssets", "Assets zip file not found");
            return;
        }
        using var zip = ZipFile.OpenRead(zipInfo.FullName);
        var root = zip.ToReadableDirectory(zipInfo.FullName);
    #endif

        Stack<(IReadableDirectory dir, IEnumerator<IReadableDirectory> children)> dirStack = new();
        // ReSharper disable once GenericEnumeratorNotDisposed
        dirStack.Push((root, root.Directories.GetEnumerator()));
        using MemoryStream buffer = new();
        while (dirStack.TryPeek(out var info))
        {
            var (dir, children) = info;
            if (children.MoveNext())
            {
                FileLoader.EnterDirectory(children.Current.Name);
                // ReSharper disable once GenericEnumeratorNotDisposed
                dirStack.Push((children.Current, children.Current.Directories.GetEnumerator()));
                continue;
            }

            foreach (var file in dir.Files)
            {
                buffer.SetLength(0);
                buffer.Position = 0;
                using var fileStream = file.OpenRead();
                fileStream.CopyTo(buffer);
                FileLoader.LoadFile(file.Name, buffer.ToArray());
            }

            FileLoader.ExitDirectory();
            children.Dispose();
            dirStack.Pop();
        }
    }


    //Process
    public static event Action<double>? Process;

    public override bool _Process(double delta)
    {
        Process?.Invoke(delta);

        var result = base._Process(delta);

        //Event Cycle
        try { InnerEventBus.HandleEvent(); }
        catch (QueueEventHandlingException e)
        {
            Logger.Log(LogLevel.Error, "EventHandling", e.Message);
            Logger.Log(LogLevel.Debug, "EventHandling", e.StackTrace ?? "no stack trace");
        }

        return result;
    }


    //Finalization
    public override void _Finalize()
    {
        Logger.Log(LogLevel.Info, "Finalizing", "Game finalizing...");

        base._Finalize();

        LogBus.Dispose();
    }


    //Public Methods
    public static void Quit() => Instance.Quit(0);


    //Logging
    private static Logger Logger => LogBus.GetLogger("Root");


    //Exceptions
    public class GameNotInitializedException : Exception;
}