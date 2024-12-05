using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Mobs;
using BabelRush.Scenery;

using Godot;

using KirisameLib.Event;
using KirisameLib.Event.Generated;

using MobInterface = BabelRush.Gui.Mobs.MobInterface;

namespace BabelRush.Tests.GamePlay;

[EventHandlerContainer]
public partial class CardUseTest : Node
{
    public override void _Ready()
    {
        SubscribeInstanceHandler(Game.EventBus);
        CallDeferred(MethodName.Initialize);
    }

    private void Initialize()
    {
        var player = MobInterface.GetInstance(new Mob(MobType.Default,  Alignment.Friend) { MaxHealth = 100, Health = 100 });
        var friend1 = MobInterface.GetInstance(new Mob(MobType.Default, Alignment.Friend) { MaxHealth = 100, Health = 100 });
        var enemy1 = MobInterface.GetInstance(new Mob(MobType.Default,  Alignment.Enemy) { MaxHealth = 100, Health = 100 });
        var enemy2 = MobInterface.GetInstance(new Mob(MobType.Default,  Alignment.Enemy) { MaxHealth = 100, Health = 100 });

        Play.Initialize(player.Mob, new Scene());
        AddChild(Play.Node);
        Play.BattleField.AddMobs(enemy1.Mob, enemy2.Mob, friend1.Mob);
        Play.PlayerState.Ap = 6;

        var scene = Play.Scene.Node;

        scene.AddChild(player);
        scene.AddChild(friend1);
        scene.AddChild(enemy1);
        scene.AddChild(enemy2);

        player.Mob.Position = GetNode<Marker2D>("Player").Position.X;
        friend1.Mob.Position = GetNode<Marker2D>("Friend1").Position.X;
        enemy1.Mob.Position = GetNode<Marker2D>("Enemy1").Position.X;
        enemy2.Mob.Position = GetNode<Marker2D>("Enemy2").Position.X;
    }

    public void AddCard()
    {
        Play.CardHub.DrawPile.AddCard(CardType.NewInstance());
    }

    private CardType CardType =>
        new CardType("test", true, Cost, TargetPatterns.Select(pattern => (new ActionType("test", pattern, []), 1)), []);

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