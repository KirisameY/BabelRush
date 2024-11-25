using System;
using System.Diagnostics.CodeAnalysis;

using Godot;

using KirisameLib.Core.Events;
using KirisameLib.Core.Logging;

namespace BabelRush;

public partial class Game : Node
{
    //Singleton & Initialize
    [field: AllowNull, MaybeNull]
    public static Game Instance
    {
        get => field ?? throw new GameNotInitializedException();
        private set;
    }

    private Game()
    {
        Instance = this;

        //Init logger
    #if TOOLS
        const LogLevel minWriteLevel = LogLevel.Debug;
        const LogLevel minPrintLevel = LogLevel.Debug;
    #else
        const LogLevel minWriteLevel = LogLevel.Info;
        const LogLevel minPrintLevel = LogLevel.Warning;
    #endif
        LogManager.Initialize(minWriteLevel, minPrintLevel, GD.Print, Project.LogDirPath, Project.Name, Project.MaxLogFileCount);
    }


    //Handle notification here
    public override void _Notification(int what)
    {
        switch ((long)what)
        {
            case NotificationWMCloseRequest:
                OnCloseRequest();
                break;
        }
    }


    //Methods
    public override void _Ready()
    {
        Logger.Log(LogLevel.Debug, "Initializing", "Debug On");
        Logger.Log(LogLevel.Info,  "Initializing", "Game Start!");
    }

    private void OnCloseRequest()
    {
        const string logProcess = "Game closing...";
        Logger.Log(LogLevel.Info, logProcess, "Close Requested");

        Logger.Log(LogLevel.Info, logProcess, "LogManager disposing...");
        LogManager.Dispose();
    }


    //Public Methods
    public static void ExitGame()
    {
        Logger.Log(LogLevel.Info, "Exiting", "Exit command received");
        Instance.PropagateNotification((int)NotificationWMCloseRequest);
        Instance.GetTree().Quit();
    }


    //Event
    public static EventBus EventBus { get; } = new ImmediateEventBus();


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("Root");


    //Exceptions
    public class GameNotInitializedException : Exception;
}