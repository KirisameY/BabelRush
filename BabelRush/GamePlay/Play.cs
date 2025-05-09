using System;

using BabelRush.Cards;
using BabelRush.Mobs;
using BabelRush.Scenery;
using BabelRush.Scenery.Collision;

using KirisameLib.Randomization;
using KirisameLib.Randomization.RandomGenerators;
using KirisameLib.Event;
using KirisameLib.Logging;

namespace BabelRush.GamePlay;

[EventHandlerContainer]
public sealed partial class Play
{
    #region Singleton & Initialize

    private Play(BattleField battleField, Scene scene, uint randomSeed)
    {
        _battleField = battleField;
        _scene = scene;
        if (randomSeed == 0) randomSeed = (uint)DateTime.Now.Ticks;
        _random = new RandomBelt<SimpleRandomGenerator>(new XorShiftGenerator(randomSeed));
        _cardHub = new(_random);
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

    public static void Initialize(Mob player, Scene scene, uint randomSeed = 0)
    {
        const string logProcess = "Initializing...";

        _instance?.Dispose();
        _instance = new(new(player), scene, randomSeed);

        Logger.Log(LogLevel.Info, logProcess, "Initializing Scene...");
        Scene.CollisionSpace.AddArea(ScreenArea);
        Node.AddChild(Scene.Node);
        Logger.Log(LogLevel.Debug, logProcess, "Subscribing process event...");
        Game.Process += Process;

        Logger.Log(LogLevel.Info, logProcess, "Gameplay initialized successfully");
    }

    #endregion


    //Dispose
    private void Dispose()
    {
        const string logProcess = "Disposing";

        Logger.Log(LogLevel.Debug, logProcess, "Unsubscribing process event...");
        Game.Process -= Process;
        Logger.Log(LogLevel.Debug, logProcess, "Free Node...");
        _node.QueueFree();
        Logger.Log(LogLevel.Info, logProcess, "Old gameplay disposed");
    }


    //Tick Loop
    public static void Process(double delta)
    {
        //player state update
        PlayerState.ProcessUpdate(delta);

        //player moving
        if (PlayerState.Moving)
            BattleField.Player.Position += PlayerState.MovingSpeed * delta;
    }


    #region Public members

    private readonly BattleField _battleField;
    public static BattleField BattleField => Instance._battleField;

    private readonly PlayNode _node = PlayNode.GetInstance();
    public static PlayNode Node => Instance._node;

    private readonly PlayerState _playerState = new();
    public static PlayerState PlayerState => Instance._playerState;

    private readonly RandomBelt _random;
    public static RandomBelt Random => Instance._random;

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
            value.Ready(Node);
        }
    }

    private readonly CardHub _cardHub;
    public static CardHub CardHub => Instance._cardHub;

    #endregion


    #region Public properties

    private readonly int _minCardValue = 4;
    public static int MinCardValue => Instance._minCardValue;

    #endregion


    //ScreenArea
    private static Area ScreenArea { get; } = new(0, Project.ViewportSize.X / 2);


    #region EventHandler

    [EventHandler]
    public static void OnPlayerMoved(SceneObjectMovedEvent e)
    {
        if (e.SceneObject != BattleField.Player) return;

        //camera
        Node.Camera.TargetPositionX = (float)e.NewPosition;

        //screen area
        float offset = Node.Camera.Offset.X; //temp
        ScreenArea.Position = e.NewPosition + offset;
    }

    [EventHandler]
    public static void OnEntityEnteredScreen(ObjectEnteredEvent e)
    {
        if (e.Object is not Mob mob) return;
        BattleField.AddMob(mob);
    }

    [EventHandler]
    public static void OnEntityExitedScreen(ObjectExitedEvent e)
    {
        if (e.Object is not Mob mob) return;
        BattleField.RemoveMob(mob);
    }

    #endregion


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger("GamePlay");


    //Exception
    public class GamePlayNotInitializedException : Exception;
}