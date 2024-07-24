using BabelRush.Gui.Mob;

using Godot;

namespace BabelRush.Tests.Mob;

public partial class MobTest : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        MobInterface = MobInterface.GetInstance(Mobs.Mob.Default);
        AddChild(MobInterface);
        MobInterface.Position = GetNode<Marker2D>("Marker2D").Position;
    }
    
    private MobInterface? MobInterface { get; set; }

    public void SetMobMaxHealth(int value)
    {
        if (MobInterface is not null) MobInterface.Mob.MaxHealth = value;
    }

    public void SetMobHealth(int value)
    {
        if (MobInterface is not null) MobInterface.Mob.Health = value;
    }
}