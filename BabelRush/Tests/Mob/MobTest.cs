using BabelRush.Gui.Mob;
using BabelRush.Mobs;

using Godot;

using KirisameLib.Events;

namespace BabelRush.Tests.Mob;

public partial class MobTest : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MobInterface = MobInterface.GetInstance(Mobs.Mob.Default);
        AddChild(MobInterface);
        MobInterface.Position = GetNode<Marker2D>("Marker2D").Position;
        EventBus.Register<MobEvent>(OnMobEvent);
        EventBus.Register<MobInterfaceEvent>(OnMobInterfaceEvent);
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
    private void OnMobInterfaceEvent(MobInterfaceEvent e)
    {
        GD.Print(e);
    }

    private void OnMobEvent(MobEvent e)
    {
        GD.Print(e);
    }
}