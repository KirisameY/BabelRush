using System;

using BabelRush.Scenery;

using Godot;

using KirisameLib.Core.Events;

using MobInterface = BabelRush.Gui.Mobs.MobInterface;

namespace BabelRush.Mobs;

public class Mob(MobType type, Alignment alignment) : VisualObject
{
    //Properties
    public MobType Type { get; } = type;

    private int _maxHealth;
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            var old = MaxHealth;
            _maxHealth = value;
            Health = Math.Max(MaxHealth, Health);
            EventBus.Publish(new MobMaxHealthChangedEvent(this, old, MaxHealth));
        }
    }

    private int _health;
    public int Health
    {
        get => _health;
        set
        {
            var old = Health;
            _health = value;
            EventBus.Publish(new MobHealthChangedEvent(this, old, Health));
        }
    }

    private Alignment _alignment = alignment;
    public Alignment Alignment
    {
        get => _alignment;
        set
        {
            if (Alignment == value) return;
            var old = Alignment;
            _alignment = value;
            EventBus.Publish(new MobAlignmentChangedEvent(this, old, Alignment));
        }
    }


    //Interface
    public override Node CreateInterface()
    {
        return MobInterface.GetInstance(this);
    }


    //Default
    public static Mob Default { get; } = new(MobType.Default, Alignment.Neutral);
}