using System.Diagnostics.CodeAnalysis;

using BabelRush.Actions;
using BabelRush.Mobs.Actions;
using BabelRush.Numerics;
using BabelRush.Scenery;

using Godot;

using KirisameLib.Event;

using MobActionInterruptedEvent = BabelRush.Mobs.Actions.MobActionInterruptedEvent;
using MobInterface = BabelRush.Gui.Mobs.MobInterface;

namespace BabelRush.Mobs;

[EventHandlerContainer]
public partial class Mob(MobType type, Alignment alignment) : VisualObject
{
    #region Properties

    public override bool Collidable => true;

    public MobType Type => type;

    [field: AllowNull, MaybeNull]
    public Numeric<int> MaxHealth => field ??=
        new Numeric<int>(type.Health)
           .WithFinalValueUpdatedHandler((_, oldValue, newValue) => Game.EventBus.Publish(new MobHealthChangedEvent(this, oldValue, newValue)))
           .WithFinalValueUpdatedHandler((_, _, newValue) => Health.Clamp = (0, newValue));

    [field: AllowNull, MaybeNull]
    public Numeric<int> Health => field ??=
        new Numeric<int>(MaxHealth) { Clamp = (0, MaxHealth) }
           .WithFinalValueUpdatedHandler((_, oldValue, newValue) => Game.EventBus.Publish(new MobMaxHealthChangedEvent(this, oldValue, newValue)));

    public MobAction? CurrentAction { get; private set; } = null; //todo: 未实现

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

    #endregion


    #region Update&Register

    protected override void _EnterScene()
    {
        SubscribeInstanceHandler(Game.EventBus);
        Game.Process += Process;
    }

    protected override void _ExitScene()
    {
        UnsubscribeInstanceHandler(Game.EventBus);
        Game.Process -= Process;
    }

    private void Process(double delta)
    {
        // UpdateAction
        if (CurrentAction is not null)
        {
            CurrentAction.Progress += delta;
        }
    }

    #endregion


    #region Public Methods

    public void SetAction(MobAction? action)
    {
        if (CurrentAction is not null) Game.EventBus.Publish(new MobActionInterruptedEvent(this, CurrentAction));
        CurrentAction = action;
        if (action is not null) Game.EventBus.Publish(new MobActionStartedEvent(this, action));
    }

    #endregion


    //Interface
    public override Node CreateInterface()
    {
        return MobInterface.GetInstance(this);
    }


    //Default
    public static Mob Default { get; } = new(MobType.Default, Alignment.Neutral);


    #region Event Handlers

    [EventHandler]
    private void OnMobActionExecuted(MobActionExecutedEvent e)
    {
        if (e.Mob != this) return;
        CurrentAction = null;
        //todo: 获取下一动作
    }

    [EventHandler]
    private void OnMobActionCancelled(MobActionCanceledEvent e)
    {
        if (e.Mob != this) return;
        CurrentAction = null;
        //todo: 获取下一动作
    }

    #endregion
}