using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Effects;
using BabelRush.GamePlay;
using BabelRush.Level.Scenery;
using BabelRush.Mobs.Actions;
using BabelRush.Numerics;
using BabelRush.Numerics.Modifiers;

using Godot;

using KirisameLib.Event;
using KirisameLib.Extensions;
using KirisameLib.Logging;

using MobActionInterruptedEvent = BabelRush.Mobs.Actions.MobActionInterruptedEvent;
using MobInterface = BabelRush.Gui.Mobs.MobInterface;

namespace BabelRush.Mobs;

[EventHandlerContainer]
public partial class Mob(MobType type, Alignment alignment) : VisualObject
{
    #region Properties

    public override bool Collidable => true;

    public MobType Type => type;

    private readonly DynamicClampModifier<int> _healthClampModifier = new(0, type.Health);

    [field: AllowNull, MaybeNull]
    public Numeric<int> MaxHealth => field ??=
        new Numeric<int>(type.Health)
           .WithModifier(new ClampModifier<int>(0, null))
           .WithFinalValueUpdatedHandler((_, oldValue, newValue) => Game.GameEventBus.Publish(new MobHealthChangedEvent(this, oldValue, newValue)))
           .WithFinalValueUpdatedHandler((_, _, newValue) => _healthClampModifier.Max = newValue);

    [field: AllowNull, MaybeNull]
    public Numeric<int> Health => field ??=
        new Numeric<int>(MaxHealth)
           .WithModifier(_healthClampModifier)
           .WithFinalValueUpdatedHandler((_, oldValue, newValue) => Game.GameEventBus.Publish(new MobMaxHealthChangedEvent(this, oldValue, newValue)));

    [field: AllowNull, MaybeNull]
    public MobActionStrategizer ActionStrategizer => field ??= Type.ActionStrategy.NewInstance(this);

    public MobAction? CurrentAction { get; private set; } = null;

    public Alignment Alignment
    {
        get;
        set
        {
            if (Alignment == value) return;
            var old = Alignment;
            field = value;
            Game.GameEventBus.Publish(new MobAlignmentChangedEvent(this, old, Alignment));
        }
    } = alignment;

    #endregion


    #region Effect

    private readonly List<Effect> _effects = [];

    [field: AllowNull, MaybeNull]
    public IReadOnlyList<Effect> Effects => field ??= _effects.AsReadOnly();

    public void ApplyEffect(Effect effect, double time) => ApplyEffectAsync(effect, time).ContinueWith(t =>
    {
        Logger.Log(LogLevel.Error, nameof(ApplyEffect), $"Exception thrown: {t.Exception?.Flatten()}");
        Logger.Log(LogLevel.Debug, nameof(ApplyEffect), $"StackTrace: {t.Exception?.StackTrace}");
    }, TaskContinuationOptions.OnlyOnFaulted);

    public async Task<double?> ApplyEffectAsync(Effect effect, double time)
    {
        if (effect.AffectedMob is not null)
        {
            Logger.Log(LogLevel.Warning, nameof(ApplyEffectAsync), $"Tried to apply an already applied effect({effect}).");
            return null;
        }

        var result = await effect.ApplyTo(this, time);
        if (result is null) return null;
        _effects.Add(effect);
        return result;
    }

    public void RemoveEffect(Effect effect, bool natural) => RemoveEffectAsync(effect, natural).ContinueWith(t =>
    {
        Logger.Log(LogLevel.Error, nameof(RemoveEffect), $"Exception thrown: {t.Exception?.Flatten()}");
        Logger.Log(LogLevel.Debug, nameof(RemoveEffect), $"StackTrace: {t.Exception?.StackTrace}");
    }, TaskContinuationOptions.OnlyOnFaulted);

    public async Task<bool> RemoveEffectAsync(Effect effect, bool natural = false)
    {
        if (effect.AffectedMob != this)
        {
            Logger.Log(LogLevel.Warning, nameof(RemoveEffectAsync), $"Tried to remove an effect({effect}) that applied to other mob.");
            return false;
        }

        var result = await effect.Remove(natural);
        if (!result) return false;
        _effects.Remove(effect);
        return true;
    }

    private async Task UpdateEffects(double delta)
    {
        List<Task<(bool removed, Effect effect)>> removeTasks = [];
        foreach (var effect in _effects)
        {
            if (!effect.ProcessUpdate(delta)) continue;
            var t = CreateRemoveTask(effect);
            removeTasks.Add(t);
        }
        if (removeTasks.Count == 0) return;

        var results = await Task.WhenAll(removeTasks);
        results.Where(result => result.removed)
               .ForEach(result => _effects.Remove(result.effect));

        return;

        static async Task<(bool removed, Effect effect)> CreateRemoveTask(Effect effect)
        {
            var result = await effect.Remove(true);
            return (result, effect);
        }
    }

    #endregion


    #region Update&Register

    protected override void _EnterScene()
    {
        SubscribeInstanceHandler(Game.GameEventBus);
        Game.Process += Process;

        _ = ActionStrategizer; // initialize strategizer
    }

    protected override void _ExitScene()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
        Game.Process -= Process;
    }

    private void Process(double delta)
    {
        // UpdateAction
        if (CurrentAction is not null)
        {
            CurrentAction.Progress += delta;
        }

        // UpdateEffect
        UpdateEffects(delta).ContinueWith(t =>
        {
            Logger.Log(LogLevel.Error, "RemovingEffect", $"Exception thrown: {t.Exception?.Flatten()}");
            Logger.Log(LogLevel.Debug, "RemovingEffect", $"StackTrace: {t.Exception?.StackTrace}");
        }, TaskContinuationOptions.OnlyOnFaulted);
    }

    #endregion


    #region Public Methods

    public void SetAction(MobAction? action)
    {
        if (CurrentAction is not null) Game.GameEventBus.Publish(new MobActionInterruptedEvent(this, CurrentAction));
        CurrentAction = action;
        if (action is not null) Game.GameEventBus.Publish(new MobActionStartedEvent(this, action));
    }

    #endregion


    //Interface
    public override Node CreateInterface()
    {
        return MobInterface.GetInstance(this);
    }

    public override float Parallax => 1;


    //Default
    public static Mob Default { get; } = new(MobType.Default, Alignment.Neutral);


    #region Event Handlers

    [EventHandler]
    private void OnMobActionExecuted(MobActionExecutedEvent e)
    {
        if (e.Mob != this) return;
        CurrentAction = null;
        SetAction(ActionStrategizer.GetNextAction());
    }

    [EventHandler]
    private void OnMobActionCancelled(MobActionCanceledEvent e)
    {
        if (e.Mob != this) return;
        CurrentAction = null;
        SetAction(ActionStrategizer.GetNextAction());
    }

    [EventHandler]
    private void OnInBattleMobAdded(InBattleMobAddedEvent e)
    {
        if (e.Mob != this) return;
        if (CurrentAction is null) SetAction(ActionStrategizer.GetNextAction());
    }

    [EventHandler]
    private void OnInBattleMobRemoved(InBattleMobRemovedEvent e)
    {
        if (e.Mob != this) return;
        SetAction(null);
    }

    #endregion


    // Logger
    private static Logger Logger { get; } = Game.LogBus.GetLogger("Mob");
}