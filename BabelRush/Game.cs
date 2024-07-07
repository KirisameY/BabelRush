using Godot;

using KirisameLib.Logging;

namespace BabelRush;

public partial class Game : Node
{
    #pragma warning disable CS8618
    //Singleton
    public static Game Instance { get; private set; }

    private Game() => Instance = this;


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
        //Init logger
    #if DEBUG
        var minWriteLevel = LogLevel.Debug;
        var minPrintLevel = LogLevel.Debug;
    #else
        var minWriteLevel = LogLevel.Info;
        var minPrintLevel = LogLevel.Warning;
    #endif
        LogManager.Initialize(minWriteLevel, minPrintLevel, GD.Print, Project.LogDirPath, Project.Name, Project.MaxLogFileCount);
        Logger = LogManager.GetLogger("Root");

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
}