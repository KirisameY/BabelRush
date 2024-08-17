using System;

using BabelRush.Mobs;
using BabelRush.Scenery;
using BabelRush.Scenery.Collision;

using JetBrains.Annotations;

using KirisameLib.Events;
using KirisameLib.Logging;

namespace BabelRush.GamePlay;

[EventHandler]
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
        const string logProcess = "Initializing...";

        _instance?.Dispose();
        _instance = new(new(player), scene);

        Logger.Log(LogLevel.Info, logProcess, "Initializing Scene...");
        Scene.CollisionSpace.AddArea(ScreenArea);
        Node.AddChild(Scene.Node);

        Logger.Log(LogLevel.Info, logProcess, "Gameplay initialized successfully");
    }


    //Dispose
    private void Dispose()
    {
        const string logProcess = "Disposing";

        Logger.Log(LogLevel.Debug, logProcess, "Free Node...");
        _node.QueueFree();
        Logger.Log(LogLevel.Info, logProcess, "Old gameplay disposed");
    }


    //Tick Loop
    public static void Process(double delta)
    {
        //player moving
        if (State.PlayerInfo.Moving)
            State.Player.Position += State.PlayerInfo.MovingSpeed * delta;
    }


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
            if (value == Scene) return;
            Instance._scene.Dispose();
            Instance._scene = value;
            value.CollisionSpace.AddArea(ScreenArea);
            Node.AddChild(value.Node);
        }
    }

    private readonly CardManager _cardManager = new();
    public static CardManager CardManager => Instance._cardManager;


    //ScreenArea
    private static Area ScreenArea { get; } = new(0, Project.ViewportSize.X / 2);


    //EventHandler
    [EventHandler] [UsedImplicitly]
    public static void OnPlayerMoved(SceneObjectMovedEvent e)
    {
        if (e.SceneObject != State.Player) return;

        //camera
        Node.Camera.TargetPositionX = (float)e.NewPosition;

        //screen area
        float offset = Node.Camera.Offset.X; //temp
        ScreenArea.Position = e.NewPosition + offset;
    }

    [EventHandler] [UsedImplicitly]
    public static void OnEntityEnteredScreen(ObjectEnteredEvent e)
    {
        if (e.Object is not Mob mob) return;
        State.AddMob(mob);
    }

    [EventHandler] [UsedImplicitly]
    public static void OnEntityExitedScreen(ObjectExitedEvent e)
    {
        if (e.Object is not Mob mob) return;
        State.RemoveMob(mob);
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger("GamePlay");


    //Exception
    public class GamePlayNotInitializedException : Exception;
}