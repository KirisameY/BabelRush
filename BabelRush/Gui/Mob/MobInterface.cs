using BabelRush.Mobs;

using Godot;

using KirisameLib.Events;
using KirisameLib.Logging;

using TheMob = BabelRush.Mobs.Mob;

namespace BabelRush.Gui.Mob;

public partial class MobInterface : Node2D
{
    //Factory
    private MobInterface() { }

    public static MobInterface GetInstance(TheMob mob)
    {
        MobInterface instance = CreateInstance();
        instance.Mob = mob;
        return instance;
    }

    private static MobInterface CreateInstance()
    {
        var instance = Scene.Instantiate<MobInterface>();
        EventHandlerRegisterer.RegisterInstance(instance);
        return instance;
    }

    private const string ScenePath = "res://Gui/Mob/Mob.tscn";
    private static PackedScene Scene { get; } = ResourceLoader.Load<PackedScene>(ScenePath);


    //Sub nodes
    private Node2D? _healthBar;
    private Node2D HealthBar => _healthBar ??= GetNode<Node2D>("HealthBar");


    //Properties
    private TheMob? _mob;
    public TheMob Mob
    {
        get
        {
            if (_mob is not null) return _mob;
            Logger.Log(LogLevel.Error, "GettingMob", $"MobInterface {this} has no mob instance reference");
            return TheMob.Default;
        }
        private set
        {
            _mob = value;
            Refresh();
        }
    }


    //Update
    private static readonly StringName StringNameMaxHealth = "max_health";
    private static readonly StringName StringNameHealth = "health";

    private int _lastMaxHealth;
    private int _lastHealth;

    private void Refresh()
    {
        CallDeferred(MethodName.UpdateHealthBar);
    }

    private void UpdateHealthBar()
    {
        if (Mob.MaxHealth != _lastMaxHealth) HealthBar.SetDeferred(StringNameMaxHealth, Mob.MaxHealth);
        if (Mob.Health != _lastHealth) HealthBar.SetDeferred(StringNameHealth,          Mob.Health);
    }


    //Event

    [EventHandler]
    private void OnMobMaxHealthChanged(MobMaxHealthChangedEvent e)
    {
        if (e.Mob == Mob)
        {
            HealthBar.SetDeferred(StringNameMaxHealth, e.NewValue);
        }
    }

    [EventHandler]
    private void OnMobHealthChanged(MobHealthChangedEvent e)
    {
        if (e.Mob == Mob)
        {
            HealthBar.SetDeferred(StringNameHealth, e.NewValue);
        }
    }


    //Signal
    private void OnMouseEntered()
    {
        EventBus.Publish(new MobInterfaceSelectedEvent(this, true));
    }

    private void OnMouseExited()
    {
        EventBus.Publish(new MobInterfaceSelectedEvent(this, false));
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(MobInterface));
}