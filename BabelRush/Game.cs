using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using BabelRush.GamePlay;
using BabelRush.I18n;
using BabelRush.Registering;
using BabelRush.Scripting;

using Godot;

using KirisameLib.Event;
using KirisameLib.Event.Generated;
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


    #region Properties

    [field: AllowNull, MaybeNull]
    public static LogBus LogBus // todo: 使Log接收来自godot的调试输出
    {
        get => field ?? throw new GameNotInitializedException();
        private set;
    }

    private static readonly DelayedEventBus InnerGameEventBus = new((ev, ex) =>
    {
        Logger.Log(LogLevel.Error, "EventHandling",
                   $"Unhandled exception thrown from game event {ev}:\n{ex}");
    });
    public static EventBus GameEventBus => InnerGameEventBus;
    public const string GameEventGroup = "";

    private static readonly ImmediateEventBus InnerLoadEventBus = new((ev, ex) =>
    {
        Logger.Log(LogLevel.Error, "EventHandling",
                   $"Unhandled exception thrown from loading event {ev}:\n{ex}");
    });
    public static EventBus LoadEventBus => InnerLoadEventBus;
    public const string LoadEventGroup = "Load";

    public static Play? Play
    {
        get;
        private set
        {
            field?.Dispose();
            field = value;
        }
    }

    public static string Local
    {
        get;
        set
        {
            if (field == value) return;
            var prev = field;
            field = value;
            RegisterManager.LoadLocalAssets(value);
            GameEventBus.Publish(new LocalChangedEvent(prev, value));
        }
    } = "zh-cn";

    #endregion


    //Initialization
    private bool _initialized = false;

    public override void _Initialize()
    {
        if (_initialized)
        {
            Logger.Log(LogLevel.Error, "Initializing", "Game class already initialized");
            return;
        }

        _initialized = true;
        Instance     = this;
        LogInitialize();

        Logger.Log(LogLevel.Info, "Initializing", "Subscribing default static event handlers...");
        GlobalEventHandlersSubscriber.Subscribe(GameEventBus);
        GlobalEventHandlersSubscriber.Subscribe(LoadEventBus, LoadEventGroup);

        base._Initialize();

        Logger.Log(LogLevel.Info, "Initializing", "Game class loaded");

        Logger.Log(LogLevel.Info, "Initializing", "Loading script frame...");
        ScriptHub.Initialize();

        Logger.Log(LogLevel.Info, "Initializing", "Loading assets...");
        RegisterManager.LoadCommonAssets();
        Logger.Log(LogLevel.Info, "Initializing", "Loading local assets...");
        RegisterManager.LoadLocalAssets("zh-cn");
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


    //Process
    public static event Action<double>? Process;

    public override bool _Process(double delta)
    {
        Process?.Invoke(delta);

        var result = base._Process(delta);

        //Event Cycle
        try { InnerGameEventBus.HandleEvent(); }
        catch (QueueEventSendingException e)
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
    public static void SetPlay(Play play)
    {
        Play = play;
        Instance.Root.AddChild(play.Node);
    }

    public static void Quit() => Instance.Quit(0);


    //Logging
    private static Logger Logger => LogBus.GetLogger("Root");


    //Exceptions
    public class GameNotInitializedException : Exception;
}