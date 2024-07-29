using System.Diagnostics.CodeAnalysis;
using System.Reflection;

using BabelRush.GamePlay;
using BabelRush.Gui.MainUI;
using BabelRush.Gui.Mob;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Tests.GamePlay;

public partial class CardUseTest : Node
{
    public override void _Ready()
    {
        EventHandlerRegisterer.RegisterInstance(this);
        CallDeferred(MethodName.Initialize);
    }

    private void Initialize()
    {
        var player = MobInterface.GetInstance(new Mobs.Mob { MaxHealth = 100, Health = 100 });
        var enemy1 = MobInterface.GetInstance(new Mobs.Mob { MaxHealth = 100, Health = 100 });
        var enemy2 = MobInterface.GetInstance(new Mobs.Mob { MaxHealth = 100, Health = 100 });

        var scene = GetNode<Node2D>("Scene");

        scene.AddChild(player);
        scene.AddChild(enemy1);
        scene.AddChild(enemy2);

        player.Position = GetNode<Marker2D>("Scene/Player").Position;
        enemy1.Position = GetNode<Marker2D>("Scene/Enemy1").Position;
        enemy2.Position = GetNode<Marker2D>("Scene/Enemy2").Position;

        Play.Initialize(player.Mob);
        AddChild(Play.Node);
    }

    public void AddCard()
    {
        var cardField = GetNode<CardField>("MainUi/CardField");
        cardField.AddCard(Cards.Card.Default);
    }

    [EventHandler] [UsedImplicitly]
    [SuppressMessage("Performance", "CA1822:将成员标记为 static")]
    private void OnEvent(BaseEvent e)
    {
        GD.Print(e);
    }
}