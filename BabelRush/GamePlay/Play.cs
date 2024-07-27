using System;

using BabelRush.Mobs;

using KirisameLib.Logging;

namespace BabelRush.GamePlay;

public class Play
{
    //Singleton & Initialize
    private Play(PlayState state) //Todo
    {
        _state = state;
        _node = new();
    }

    private static Play? _instance;
    public static Play Instance
    {
        get
        {
            if (_instance is not null) return _instance;
            Logger.Log(LogLevel.Error, "GettingInstance", "GamePlay did not initialized");
            throw new GamePlayNotInitializedException();
        }
    }

    public static void Initialize(Mob player)
    {
        const string logProcess = "Initializing";

        _instance?.Dispose();
        _instance = new(new(player));
        Logger.Log(LogLevel.Info, logProcess, "Gameplay initialized successfully");
    }


    //Member
    private readonly PlayState _state;
    public static PlayState State => Instance._state;

    private readonly PlayNode _node;
    public static PlayNode Node => Instance._node;


    //Dispose
    private void Dispose()
    {
        const string logProcess = "Disposing";

        Logger.Log(LogLevel.Debug, logProcess, "Free Node...");
        _node.QueueFree();
        Logger.Log(LogLevel.Debug, logProcess, "Old gameplay disposed");
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("GamePlay");


    //Exception
    public class GamePlayNotInitializedException : Exception;
}