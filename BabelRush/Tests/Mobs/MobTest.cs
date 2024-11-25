using BabelRush.Gui.Mobs;
using BabelRush.Mobs;

using Godot;

using KirisameLib.Core.Events;

using MobInterface = BabelRush.Gui.Mobs.MobInterface;

namespace BabelRush.Tests.Mobs;

[EventHandlerContainer]
public partial class MobTest : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MobInterface = MobInterface.GetInstance(Mob.Default);
        AddChild(MobInterface);
        MobInterface.Position = GetNode<Marker2D>("Marker2D").Position;
        SubscribeInstanceHandler(Game.EventBus);
    }


    //Testing
    private MobInterface? MobInterface { get; set; }

    public void SetMobMaxHealth(int value)
    {
        if (MobInterface is not null) MobInterface.Mob.MaxHealth = value;
    }

    public void SetMobHealth(int value)
    {
        if (MobInterface is not null) MobInterface.Mob.Health = value;
    }


    //Event
    [EventHandler]
    private void OnMobInterfaceEvent(MobInterfaceEvent e)
    {
        GD.Print(e);
    }

    [EventHandler]
    private void OnMobEvent(MobEvent e)
    {
        GD.Print(e);
    }
}