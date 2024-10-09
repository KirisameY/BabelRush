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


    //Cost
    public int MaxAp { get; } = 6;
    public int Ap { get; set; }


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