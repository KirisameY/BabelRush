using System;

using BabelRush.Mobs;
using BabelRush.Scenery;

using KirisameLib.Logging;

namespace BabelRush.GamePlay;

public class Play
{
    //Singleton & Initialize
    private Play(PlayState state, Scene scene)
    {
        _state = state;
        _scene = scene;
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

    public static void Initialize(Mob player, Scene scene)
    {
        const string logProcess = "Initializing";

        _instance?.Dispose();
        _instance = new(new(player), scene);
        Logger.Log(LogLevel.Info, logProcess, "Gameplay initialized successfully");
    }


    //Tick Loop
    public static void Process(double delta) { }


    //Member
    private readonly PlayState _state;
    public static PlayState State => Instance._state;

    private readonly PlayNode _node = PlayNode.GetInstance(Process);
    public static PlayNode Node => Instance._node;

    private Scene _scene;
    public static Scene Scene
    {
        get => Instance._scene;
        set
        {
            Instance._scene.Dispose();
            Instance._scene = value;
        }
    }


    //Dispose
    private void Dispose()
    {
        const string logProcess = "Disposing";

        Logger.Log(LogLevel.Debug, logProcess, "Free Node...");
        _node.QueueFree();
        Logger.Log(LogLevel.Info, logProcess, "Old gameplay disposed");
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("GamePlay");


    //Exception
    public class GamePlayNotInitializedException : Exception;
}