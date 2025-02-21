using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using BabelRush.Mobs;
using BabelRush.Mobs.Animation;

using Godot;

using KirisameLib.Event;
using KirisameLib.Logging;

namespace BabelRush.Gui.Mobs;

[EventHandlerContainer]
public partial class MobInterface : Node2D
{
    #region Factory

    private MobInterface() { }

    public static MobInterface GetInstance(Mob mob)
    {
        MobInterface instance = CreateInstance();
        instance.Mob = mob;
        instance.CallDeferred(MethodName.GettingInitialize);
        return instance;
    }

    private void GettingInitialize()
    {
        Sprite.SpriteFrames = Mob.Type.AnimationSet.SpriteFrames;
        _ = PlayAnimation(AnimateState);
    }

    private static MobInterface CreateInstance()
    {
        var instance = Scene.Instantiate<MobInterface>();
        instance.CallDeferred(MethodName.CreatingInitialize);
        return instance;
    }

    private void CreatingInitialize()
    {
        BoxShapeNode.Shape = BoxShapeNode.Shape.Duplicate() as Shape2D;
    }

    private const string ScenePath = "res://Gui/Mobs/Mob.tscn";
    private static PackedScene Scene { get; } = ResourceLoader.Load<PackedScene>(ScenePath);

    #endregion


    #region Sub nodes

    private Node2D? _healthBar;
    private Node2D HealthBar => _healthBar ??= GetNode<Node2D>("HealthBar");

    private AnimatedSprite2D? _sprite;
    private AnimatedSprite2D Sprite => _sprite ??= GetNode<AnimatedSprite2D>("Sprite");

    private Area2D? _box;
    private Area2D Box => _box ??= GetNode<Area2D>("Box");

    private CollisionShape2D? _boxShapeNode;
    private CollisionShape2D BoxShapeNode => _boxShapeNode ??= GetNode<CollisionShape2D>("Box/Shape");

    private RectangleShape2D? _boxShape;
    private RectangleShape2D BoxShape => _boxShape ??= (RectangleShape2D)BoxShapeNode.Shape;

    #endregion


    #region Properties

    [field: AllowNull, MaybeNull]
    public Mob Mob
    {
        get
        {
            if (field is not null) return field;
            Logger.Log(LogLevel.Error, "GettingMob", $"MobInterface {this} has no mob instance reference");
            return Mob.Default;
        }
        private set
        {
            field = value;
            Refresh();
        }
    }

    [field: AllowNull, MaybeNull]
    private MobAnimationId AnimateState
    {
        get { return field ??= MobAnimationId.Default; }
        set;
    }

    #endregion


    #region Update

    private static readonly StringName StringNameMaxHealth = "max_health";
    private static readonly StringName StringNameHealth = "health";

    private int _lastMaxHealth;
    private int _lastHealth;

    public override void _Process(double delta)
    {
        Position = new((float)Mob.Position, Position.Y);
    }

    private void Refresh()
    {
        CallDeferred(MethodName.UpdateHealthBar);
    }

    private void UpdateHealthBar()
    {
        if (Mob.MaxHealth != _lastMaxHealth) HealthBar.SetDeferred(StringNameMaxHealth, Mob.MaxHealth.FinalValue);
        if (Mob.Health != _lastHealth) HealthBar.SetDeferred(StringNameHealth,          Mob.Health.FinalValue);
    }

    #endregion


    #region Animation

    private async Task PlayAnimation(MobAnimationId id)
    {
        var animationSet = Mob.Type.AnimationSet;
        id = animationSet.BackToExist(id, out var info);

        //case state
        if (!id.IsAction)
        {
            AnimateState = id;
            PlayIt(id, info);
            return;
        }

        //case action
        //play before
        if (info.Start is not null)
        {
            await PlayAnimation(info.Start);
        }

        //Play this
        PlayIt(id, info);
        await ToSignal(Sprite, AnimatedSprite2D.SignalName.AnimationFinished);

        //play after
        if (info.End is not null)
        {
            await PlayAnimation(info.End);
        }

        //reset
        _ = PlayAnimation(AnimateState);

        void PlayIt(MobAnimationId aId, MobAnimationSet.AnimationInfo aInfo)
        {
            BoxShape.Size = aInfo.BoxSize;
            BoxShapeNode.Position = new(0, -aInfo.BoxSize.Y / 2f);
            Sprite.Offset = aInfo.Offset;
            Sprite.Play(aId);
        }
    }

    #endregion


    #region Event Handlers

    [EventHandler]
    private void OnMobMaxHealthChanged(MobMaxHealthChangedEvent e)
    {
        if (e.Mob != Mob) return;
        HealthBar.SetDeferred(StringNameMaxHealth, e.NewValue);
    }

    [EventHandler]
    private void OnMobHealthChanged(MobHealthChangedEvent e)
    {
        if (e.Mob != Mob) return;
        HealthBar.SetDeferred(StringNameHealth, e.NewValue);
    }

    [EventHandler]
    private void OnMobSelected(MobSelectedEvent e)
    {
        if (e.Mob != Mob) return;
        Modulate = e.ByCursor
            ? new Color(Modulate.R, e.Selected ? 0 : 1, Modulate.B)
            : new Color(Modulate.R, Modulate.G,         e.Selected ? 0 : 1);
    }

    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.EventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.EventBus);
    }

    #endregion


    #region Signal Methods

    private void OnMouseEntered()
    {
        Game.EventBus.Publish(new MobInterfaceSelectedEvent(this, true));
    }

    private void OnMouseExited()
    {
        Game.EventBus.Publish(new MobInterfaceSelectedEvent(this, false));
    }

    #endregion


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(MobInterface));
}