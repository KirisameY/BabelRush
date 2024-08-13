using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

using BabelRush.Actions;
using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Gui.Mob;
using BabelRush.Mobs;
using BabelRush.Scenery;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Tests.GamePlay;

public partial class CardUseTest : Node
{
    public override void _Ready()
    {
        EventHandlerSubscriber.SubscribeStaticIn(Assembly.GetAssembly(typeof(CardUseTest))!);
        EventHandlerSubscriber.InstanceSubscribe(this);
        CallDeferred(MethodName.Initialize);
    }

    private void Initialize()
    {
        var player = MobInterface.GetInstance(new Mobs.Mob(MobType.Default,  Alignment.Friend) { MaxHealth = 100, Health = 100 });
        var friend1 = MobInterface.GetInstance(new Mobs.Mob(MobType.Default, Alignment.Friend) { MaxHealth = 100, Health = 100 });
        var enemy1 = MobInterface.GetInstance(new Mobs.Mob(MobType.Default,  Alignment.Enemy) { MaxHealth = 100, Health = 100 });
        var enemy2 = MobInterface.GetInstance(new Mobs.Mob(MobType.Default,  Alignment.Enemy) { MaxHealth = 100, Health = 100 });

        Play.Initialize(player.Mob, new Scene());
        AddChild(Play.Node);
        Play.State.AddMobs(enemy1.Mob, enemy2.Mob, friend1.Mob);

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
        Play.State.PlayerInfo.CardField.AddCard(CardType.NewInstance());
    }

    private CardType CardType =>
        new CommonCardType("test", true, 1, TargetPatterns.Select(pattern => new ActionType("test", pattern, [])), []);
    private TargetPattern[] TargetPatterns { get; } = [new TargetPattern.None(), new TargetPattern.None()];

    public void SetTarget1(string target) =>
        TargetPatterns[0] = TargetPattern.FromString(target);

    public void SetTarget2(string target) =>
        TargetPatterns[1] = TargetPattern.FromString(target);


    //Events
    [EventHandler] [UsedImplicitly]
    [SuppressMessage("Performance", "CA1822:将成员标记为 static")]
    private void OnEvent(BaseEvent e)
    {
        GD.Print(e);
    }
}