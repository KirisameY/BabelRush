using System;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Numerics;
using BabelRush.Scenery;
using BabelRush.Utils;

using Godot;

using MobInterface = BabelRush.Gui.Mobs.MobInterface;

namespace BabelRush.Mobs;

public class Mob(MobType type, Alignment alignment) : VisualObject
{
    //Properties
    public MobType Type { get; } = type;

    [field: AllowNull, MaybeNull]
    public Numeric<int> MaxHealth => field ??=
        new Numeric<int>(type.Health)
           .WithFinalValueUpdatedHandler((_, oldValue, newValue) => Game.EventBus.Publish(new MobHealthChangedEvent(this, oldValue, newValue)))
           .WithFinalValueUpdatedHandler((_, _, newValue) => Health.Clamp = (0, newValue));

    [field: AllowNull, MaybeNull]
    public Numeric<int> Health => field ??=
        new Numeric<int>(MaxHealth) { Clamp = (0, MaxHealth) }
           .WithFinalValueUpdatedHandler((_, oldValue, newValue) => Game.EventBus.Publish(new MobMaxHealthChangedEvent(this, oldValue, newValue)));

    public Alignment Alignment
    {
        get;
        set
        {
            if (Alignment == value) return;
            var old = Alignment;
            field = value;
            Game.EventBus.Publish(new MobAlignmentChangedEvent(this, old, Alignment));
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