using BabelRush.Gui.Mob;
using BabelRush.Scenery;

using Godot;

using KirisameLib.Events;

namespace BabelRush.Mobs;

public class Mob : SceneObject
{
    private int _maxHealth;
    private int _health;
    public int MaxHealth
    {
        get => _maxHealth;
        set
        {
            var old = MaxHealth;
            _maxHealth = value;
            EventBus.Publish(new MobMaxHealthChangedEvent(this, old, MaxHealth));
        }
    }
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

    public static Mob Default { get; } = new();


    //Interface
    public override Node CreateInterface()
    {
        return MobInterface.GetInstance(this);
    }
}