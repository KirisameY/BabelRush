using System;

using Godot;

using KirisameLib.Logging;

namespace BabelRush;

public partial class Game : Node
{
    //Singleton
    private static Game? _instance;
    public static Game Instance
    {
        get => _instance ?? throw new GameNotInitializedException();
        private set => _instance = value;
    }

    private Game()
    {
        Instance = this;

        //Init logger
    #if DEBUG
        const LogLevel minWriteLevel = LogLevel.Debug;
        const LogLevel minPrintLevel = LogLevel.Debug;
    #else
        const LogLevel minWriteLevel = LogLevel.Info;
        const LogLevel minPrintLevel = LogLevel.Warning;
    #endif
        LogManager.Initialize(minWriteLevel, minPrintLevel, GD.Print, Project.LogDirPath, Project.Name, Project.MaxLogFileCount);
        Logger = LogManager.GetLogger("Root");
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

    //On Game Start
    public override void _Ready()
    {
        Logger.Log(LogLevel.Debug, "Initializing", "Debug On");
        Logger.Log(LogLevel.Info,  "Initializing", "Game Start!");
    }

    //Use this to stop game
    public void ExitGame()
    {
        PropagateNotification((int)NotificationWMCloseRequest);
        GetTree().Quit();
    }

    //Before Game Exit
    private void OnCloseRequest()
    {
        LogManager.Dispose();
    }

    //members
    private Logger Logger { get; set; }

    //Exceptions
    public class GameNotInitializedException : Exception;
}