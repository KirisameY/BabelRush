using System;

using BabelRush.Scenery;

using Godot;

using MobInterface = BabelRush.Gui.Mobs.MobInterface;

namespace BabelRush.Mobs;

public class Mob(MobType type, Alignment alignment) : VisualObject
{
    //Properties
    public MobType Type { get; } = type;

    public int MaxHealth
    {
        get;
        set
        {
            var old = MaxHealth;
            field = value;
            Health = Math.Max(MaxHealth, Health);
            GameNode.EventBus.Publish(new MobMaxHealthChangedEvent(this, old, MaxHealth));
        }
    }

    public int Health
    {
        get;
        set
        {
            var old = Health;
            field = value;
            GameNode.EventBus.Publish(new MobHealthChangedEvent(this, old, Health));
        }
    }

    public Alignment Alignment
    {
        get;
        set
        {
            if (Alignment == value) return;
            var old = Alignment;
            field = value;
            GameNode.EventBus.Publish(new MobAlignmentChangedEvent(this, old, Alignment));
        }
    } = alignment;


    //Interface
    public override Node CreateInterface()
    {
        return MobInterface.GetInstance(this);
    }


    //Default
    public static Mob Default { get; } = new(MobType.Default, Alignment.Neutral);
}