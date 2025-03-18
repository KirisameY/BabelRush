using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

using BabelRush.Mobs;
using BabelRush.Mobs.Actions;
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

    [field: AllowNull, MaybeNull]
    private Node2D HealthBar => field ??= GetNode<Node2D>("HealthBar");

    [field: AllowNull, MaybeNull]
    private Node2D ActionBar => field ??= GetNode<Node2D>("Actionbar");

    [field: AllowNull, MaybeNull]
    private AnimatedSprite2D Sprite => field ??= GetNode<AnimatedSprite2D>("Sprite");

    [field: AllowNull, MaybeNull]
    private Area2D Box => field ??= GetNode<Area2D>("Box");

    [field: AllowNull, MaybeNull]
    private CollisionShape2D BoxShapeNode => field ??= GetNode<CollisionShape2D>("Box/Shape");

    [field: AllowNull, MaybeNull]
    private RectangleShape2D BoxShape => field ??= (RectangleShape2D)BoxShapeNode.Shape;

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


    private static class Names
    {
        //Methods
        public static readonly StringName SetProgress = "set_progress";
        public static readonly StringName SetIcon = "set_icon";

        //Properties
        public static readonly StringName MaxHealth = "max_health";
        public static readonly StringName Health = "health";
        public static readonly StringName ActionValue = "action_value";
        public static readonly StringName ProgressColor = "progress_color"; // todo: 也许终有一天它会实装
    }


    #region Update

    private int _lastMaxHealth;
    private int _lastHealth;

    private bool _actionDirty;

    public override void _Process(double delta)
    {
        Position = new((float)Mob.Position, Position.Y);
        UpdateActionBarProgress();
        if (_actionDirty) UpdateActionBar();
    }

    private void Refresh()
    {
        CallDeferred(MethodName.UpdateHealthBar);
        CallDeferred(MethodName.UpdateActionBar);
    }

    private void UpdateHealthBar()
    {
        if (Mob.MaxHealth != _lastMaxHealth) HealthBar.SetDeferred(Names.MaxHealth, Mob.MaxHealth.FinalValue);
        if (Mob.Health != _lastHealth) HealthBar.SetDeferred(Names.Health,          Mob.Health.FinalValue);
    }

    private void UpdateActionBar()
    {
        _actionDirty = false;
        if (Mob.CurrentAction is not { } action)
        {
            ActionBar.Visible = false;
            return;
        }

        ActionBar.Visible = true;
        ActionBar.SetDeferred(Names.ActionValue, action.Action.Value);
        ActionBar.CallDeferred(Names.SetIcon, action.Action.Type.Icon);
        UpdateActionBarProgress();
    }

    private void UpdateActionBarProgress()
    {
        if (!ActionBar.Visible || Mob.CurrentAction is not { } action) return;
        ActionBar.CallDeferred(Names.SetProgress, action.Progress / action.Time);
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
            PlayIt(this, id, info);
            return;
        }

        //case action
        //play before
        if (info.Start is not null)
        {
            await PlayAnimation(info.Start);
        }

        //Play this
        PlayIt(this, id, info);
        await ToSignal(Sprite, AnimatedSprite2D.SignalName.AnimationFinished);

        //play after
        if (info.End is not null)
        {
            await PlayAnimation(info.End);
        }

        //reset
        _ = PlayAnimation(AnimateState);

        return;


        static void PlayIt(MobInterface mob, MobAnimationId aId, MobAnimationSet.AnimationInfo aInfo)
        {
            mob.BoxShape.Size = aInfo.BoxSize;
            mob.BoxShapeNode.Position = new(0, -aInfo.BoxSize.Y / 2f);
            mob.Sprite.Offset = aInfo.Offset;
            mob.Sprite.Play(aId);
        }
    }

    #endregion


    #region Event Handlers

    [EventHandler]
    private void OnMobMaxHealthChanged(MobMaxHealthChangedEvent e)
    {
        if (e.Mob != Mob) return;
        HealthBar.SetDeferred(Names.MaxHealth, e.NewValue);
    }

    [EventHandler]
    private void OnMobHealthChanged(MobHealthChangedEvent e)
    {
        if (e.Mob != Mob) return;
        HealthBar.SetDeferred(Names.Health, e.NewValue);
    }

    [EventHandler]
    private void OnMobSelected(MobSelectedEvent e)
    {
        if (e.Mob != Mob) return;
        Modulate = e.ByCursor
            ? new Color(Modulate.R, e.Selected ? 0 : 1, Modulate.B)
            : new Color(Modulate.R, Modulate.G,         e.Selected ? 0 : 1);
    }

    [EventHandler]
    private void OnMobActionEvent(MobActionEvent e)
    {
        if (e.Mob != Mob) return;
        _actionDirty = true;
    }

    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.GameEventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
    }

    #endregion


    #region Signal Methods

    private void OnMouseEntered()
    {
        Game.GameEventBus.Publish(new MobInterfaceSelectedEvent(this, true));
    }

    private void OnMouseExited()
    {
        Game.GameEventBus.Publish(new MobInterfaceSelectedEvent(this, false));
    }

    #endregion


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(MobInterface));
}