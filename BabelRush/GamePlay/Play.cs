using System;
using System.Runtime.CompilerServices;

using BabelRush.Cards;
using BabelRush.Mobs;
using BabelRush.Scenery;
using BabelRush.Scenery.Collision;
using BabelRush.Stages;

using KirisameLib.Randomization;
using KirisameLib.Randomization.RandomGenerators;
using KirisameLib.Event;
using KirisameLib.Logging;

namespace BabelRush.GamePlay;

[EventHandlerContainer]
public sealed partial class Play : IDisposable
{
    #region Initialize & Dispose

    private Play(BattleField battleField, Stage initialStage, uint randomSeed)
    {
        BattleField = battleField;
        Random      = new RandomBelt<SimpleRandomGenerator>(new XorShiftGenerator(randomSeed));
        CardHub     = new(Random);
        Scene       = null!; // will be set by Stage
        Stage       = initialStage;

        Game.Process += Process;
        SubscribeInstanceHandler(Game.GameEventBus);
    }

    public static Play Create(Mob player, Stage initialStage, uint randomSeed = 0)
    {
        if (randomSeed == 0) randomSeed = Unsafe.BitCast<int, uint>(DateTime.Now.Ticks.GetHashCode());
        return new(new(player), initialStage, randomSeed);
    }

    public void Dispose()
    {
        const string logProcess = "Disposing";

        Logger.Log(LogLevel.Debug, logProcess, "Unsubscribing process & events...");
        UnsubscribeInstanceHandler(Game.GameEventBus);
        Game.Process -= Process;
        Logger.Log(LogLevel.Debug, logProcess, "Free Node...");
        Node.QueueFree();
        Logger.Log(LogLevel.Info, logProcess, "Old gameplay disposed");
    }

    #endregion


    //Tick Loop
    public void Process(double delta)
    {
        //player state update
        PlayerState.ProcessUpdate(delta);

        //player moving
        if (PlayerState.Moving)
            BattleField.Player.Position += PlayerState.MovingSpeed * delta;
    }


    #region Properties

    public BattleField BattleField { get; }

    public PlayNode Node { get; } = PlayNode.CreateInstance();

    public PlayerState PlayerState { get; } = new();

    public RandomBelt Random { get; }

    public Stage Stage
    {
        get;
        set
        {
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            field?.Dispose(); // When initializing it will be null
            field = value;
            Scene = value.CreateScene();
        }
    }

    public Scene Scene
    {
        get;
        private set
        {
            if (value == field) return;
            // ReSharper disable once ConditionalAccessQualifierIsNonNullableAccordingToAPIContract
            field?.Dispose(); // When initializing it will be null
            field = value;
            value.CollisionSpace.AddArea(_screenArea);
            value.Ready(Node);
        }
    }

    public CardHub CardHub { get; }

    private readonly Area _screenArea = new(0, Project.ViewportSize.X / 2);

    // Settings
    public static int MinCardValue => 4;

    #endregion


    #region EventHandler

    [EventHandler]
    public void OnPlayerMoved(SceneObjectMovedEvent e)
    {
        if (e.SceneObject != BattleField.Player) return;

        //camera
        Node.Camera.TargetPositionX = (float)e.NewPosition + Project.ViewportSize.X / 2;

        //screen area
        float offset = Node.Camera.Offset.X; //temp
        _screenArea.Position = e.NewPosition + offset;
    }

    [EventHandler]
    public void OnEntityEntered(ObjectEnteredEvent e)
    {
        if (e.Area != _screenArea || e.Object is not Mob mob) return;
        BattleField.AddMob(mob);
    }

    [EventHandler]
    public void OnEntityExited(ObjectExitedEvent e)
    {
        if (e.Area != _screenArea || e.Object is not Mob mob) return;
        BattleField.RemoveMob(mob);
    }

    #endregion


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger("GamePlay");


    //Exception
    public class GamePlayNotInitializedException : Exception;
}