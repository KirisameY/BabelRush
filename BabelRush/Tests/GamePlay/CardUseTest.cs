using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Mobs;
using BabelRush.Scenery.Rooms;
using BabelRush.Scenery.Stages;

using Godot;

using KirisameLib.Event;

using Stage = BabelRush.Scenery.Stages.Stage;

namespace BabelRush.Tests.GamePlay;

[EventHandlerContainer]
public partial class CardUseTest : Node
{
    public override void _Ready()
    {
        SubscribeInstanceHandler(Game.GameEventBus);
        CallDeferred(MethodName.Initialize);
    }

    private void Initialize()
    {
        var player = new Mob(MobType.Default,  Alignment.Friend);
        var friend1 = new Mob(MobType.Default, Alignment.Friend);
        var enemy1 = new Mob(MobType.Default,  Alignment.Enemy);
        var enemy2 = new Mob(MobType.Default,  Alignment.Enemy);

        var play = Play.Create(player, Stage.Default);
        Game.SetPlay(play);
        play.BattleField.AddMobs(enemy1, enemy2, friend1);
        play.PlayerState.Ap = 6;

        play.Scene.AddObject(player);
        play.Scene.AddObject(friend1);
        play.Scene.AddObject(enemy1);
        play.Scene.AddObject(enemy2);

        player.Position  = GetNode<Marker2D>("Player").Position.X;
        friend1.Position = GetNode<Marker2D>("Friend1").Position.X;
        enemy1.Position  = GetNode<Marker2D>("Enemy1").Position.X;
        enemy2.Position  = GetNode<Marker2D>("Enemy2").Position.X;
    }

    public void AddCard()
    {
        Game.Play!.CardHub.DrawPile.AddCard(CardType.NewInstance());
    }

    private CardType CardType =>
        new CardType("test:test", "test:test", true, Cost, TargetPatterns.Select(pattern => (new ActionType("test:test", "test:test", pattern, []), 1)), []);

    private string[] TargetPatternNames { get; } = ["None", "None"];

    private IEnumerable<TargetPattern> TargetPatterns => TargetPatternNames.Select(TargetPattern.FromString);

    public void SetTarget1(string value) => TargetPatternNames[0] = value;

    public void SetTarget2(string value) => TargetPatternNames[1] = value;

    private int Cost { get; set; }
    public void SetCost(int value) => Cost = value;


    //Events
    [EventHandler]
    private void OnEvent(BaseEvent e)
    {
        GD.Print($"{DateTime.Now:hh:mm:ss.fff} Event: {e}");
    }

    [EventHandler]
    private void OnCardUsed(CardUsedEvent e)
    {
        e.ToExhaust.Value = true;
    }
}