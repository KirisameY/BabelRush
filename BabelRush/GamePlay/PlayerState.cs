using System.Linq;

using BabelRush.Cards;
using BabelRush.Mobs;

using KirisameLib.Core.Events;

namespace BabelRush.GamePlay;

[EventHandlerContainer]
public partial class PlayerState
{
    //Action
    public double MovingSpeed { get; set; } = 16;
    public bool Moving { get; set; }


    //Ap
    public int MaxAp { get; set; } = 6;
    public int Ap
    {
        get;
        set
        {
            if (field == value) return;
            var oldValue = field;
            field = value;
            GameNode.EventBus.Publish(new ApChangedEvent(oldValue, field));
        }
    }

    public double ApRegenerated
    {
        get;
        set
        {
            field = value;
            if (value < 1) return;
            if (Ap < MaxAp)
            {
                field -= 1;
                Ap++;
            }
            else
            {
                field = 1;
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
    [EventHandler]
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

    [EventHandler]
    private static void OnMobAdded(MobAddedEvent e)
    {
        if (e.Mob.Type.BlocksMovement && e.Mob.Alignment == Alignment.Enemy)
            Play.PlayerState.Moving = false;
    }

    [EventHandler]
    private static void OnMobRemoved(MobRemovedEvent e)
    {
        if (e.Mob.Type.BlocksMovement && e.Mob.Alignment == Alignment.Enemy
         && Play.BattleField.Enemies.All(mob => !mob.Type.BlocksMovement))
            Play.PlayerState.Moving = true;
    }


    //Card - AP
    [EventHandler]
    private static void BeforeCardUse(BeforeCardUseEvent e)
    {
        if (e.Card.Cost > Play.PlayerState.Ap) e.Cancel.Cancel();
    }


    [EventHandler]
    private static void OnCardUsed(CardUsedEvent e)
    {
        if (e.CostAp) Play.PlayerState.Ap -= e.Card.Cost;
    }

    #endregion
}