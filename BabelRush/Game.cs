using Godot;

using KirisameLib.Logging;

namespace BabelRush;

public partial class Game : Node
{
    //Singleton
    #pragma warning disable CS8618
    public static Game Instance { get; private set; }
    #pragma warning restore CS8618

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

    //Log
    public Logger Logger { get; } = new(GD.Print, Project.LogDirPath, Project.Name, Project.MaxLogFileCount);

    //On Game Start
    public override void _Ready()
    {
        Logger.Log(new(LogLevel.Info, "Start!"));
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
        Logger.Dispose();
    }
}