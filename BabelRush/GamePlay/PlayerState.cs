using System.Linq;

using BabelRush.Cards;
using BabelRush.Mobs;

using JetBrains.Annotations;

using KirisameLib.Core.Events;

namespace BabelRush.GamePlay;

[EventHandler]
public class PlayerState
{
    //Action
    public double MovingSpeed { get; set; } = 16;
    public bool Moving { get; set; }


    //Ap
    public int MaxAp { get; set; } = 6;
    private int _ap;
    public int Ap
    {
        get => _ap;
        set
        {
            if (_ap == value) return;
            var oldValue = _ap;
            _ap = value;
            EventBus.Publish(new ApChangedEvent(oldValue, _ap));
        }
    }

    private double _apRegenerated;
    public double ApRegenerated
    {
        get => _apRegenerated;
        set
        {
            _apRegenerated = value;
            if (value < 1) return;
            if (Ap < MaxAp)
            {
                _apRegenerated -= 1;
                Ap++;
            }
            else
            {
                _apRegenerated = 1;
            }
        }
    }
    public double ApRegeneration { get; set; } = 1;


    //Update
    public void ProcessUpdate(double delta)
    {
        ApRegenerated += delta * ApRegeneration;
    }


    #region Event Handlers

    //Move block
    [EventHandler] [UsedImplicitly]
    private static void OnMobAlignmentChanged(MobAlignmentChangedEvent e)
    {
        if (e.Mob.Type.BlocksMovement == false) return;
        Play.PlayerState.Moving = (e.OldAlignment, e.NewAlignment) switch
        {
            (Alignment.Enemy, not Alignment.Enemy) => true,
            (not Alignment.Enemy, Alignment.Enemy) => false,
            _                                      => Play.PlayerState.Moving
        };
    }

    [EventHandler] [UsedImplicitly]
    private static void OnMobAdded(MobAddedEvent e)
    {
        if (e.Mob.Type.BlocksMovement && e.Mob.Alignment == Alignment.Enemy)
            Play.PlayerState.Moving = false;
    }

    [EventHandler] [UsedImplicitly]
    private static void OnMobRemoved(MobRemovedEvent e)
    {
        if (e.Mob.Type.BlocksMovement && e.Mob.Alignment == Alignment.Enemy
         && Play.BattleField.Enemies.All(mob => !mob.Type.BlocksMovement))
            Play.PlayerState.Moving = true;
    }


    //Card - AP
    [EventHandler] [UsedImplicitly]
    private static void BeforeCardUse(BeforeCardUseEvent e)
    {
        if (e.Card.Cost > Play.PlayerState.Ap) e.Cancel.Cancel();
    }


    [EventHandler] [UsedImplicitly]
    private static void OnCardUsed(CardUsedEvent e)
    {
        if (e.CostAp) Play.PlayerState.Ap -= e.Card.Cost;
    }

    #endregion
}