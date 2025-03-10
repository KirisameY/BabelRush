using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;

using BabelRush.Registering;
using BabelRush.Registering.FileLoading;
using BabelRush.Scripting;

using Godot;

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

    // todo: Local
    // public static class Localization
    // {
    //     static Localization()
    //     {
    //         LocalizedRegister.LocalChangedEvent += static (prev, current) => EventBus.Publish(new LocalChangedEvent(prev, current));
    //     }
    //
    //     public static string Local
    //     {
    //         get => LocalizedRegister.Local;
    //         set => LocalizedRegister.Local = value;
    //     }
    // }


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

        Logger.Log(LogLevel.Info, "Initializing", "Loading assets...");
        //todo: 现在正是不得不把它拿走之时
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