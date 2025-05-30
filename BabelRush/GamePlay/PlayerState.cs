using System.Linq;

using BabelRush.Cards;
using BabelRush.Mobs;

using KirisameLib.Event;

namespace BabelRush.GamePlay;

[EventHandlerContainer]
public partial class PlayerState
{
    //Action
    public double MovingSpeed { get; set; } = 16;
    public bool IsBlocked { get; set; } = false;
    public bool WantMove { get; set; } = true;
    public bool IsMoving => WantMove && !IsBlocked;


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
            Game.GameEventBus.Publish(new ApChangedEvent(oldValue, field));
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
    public void ProcessUpdate(double delta) //todo: 感觉不太对，用事件解耦一下吧回头
    {
        ApRegenerated += delta * ApRegeneration;
    }


    #region Event Handlers

    //Move block
    [EventHandler]
    private static void OnMobAlignmentChanged(MobAlignmentChangedEvent e)
    {
        if (e.Mob.Type.BlocksMovement == false) return;
        Game.Play!.PlayerState.IsBlocked = (e.OldAlignment, e.NewAlignment) switch
        {
            (Alignment.Enemy, not Alignment.Enemy) => false,
            (not Alignment.Enemy, Alignment.Enemy) => true,
            _                                      => Game.Play.PlayerState.IsBlocked
        };
    }

    [EventHandler]
    private static void OnInBattleMobAdded(InBattleMobAddedEvent e)
    {
        if (e.Mob.Type.BlocksMovement && e.Mob.Alignment == Alignment.Enemy)
            Game.Play!.PlayerState.IsBlocked = true;
    }

    [EventHandler]
    private static void OnInBattleMobRemoved(InBattleMobRemovedEvent e)
    {
        if (e.Mob.Type.BlocksMovement && e.Mob.Alignment == Alignment.Enemy
         && Game.Play!.BattleField.Enemies.All(mob => !mob.Type.BlocksMovement))
            Game.Play.PlayerState.IsBlocked = false;
    }


    //Card - AP
    [EventHandler]
    private static void OnCardUseRequest(CardUseRequestEvent e)
    {
        if (e.Card.Cost > Game.Play!.PlayerState.Ap) e.Cancel.Cancel();
    }


    [EventHandler]
    private static void OnCardUsed(CardUsedEvent e)
    {
        if (e.CostAp) Game.Play!.PlayerState.Ap -= e.Card.Cost;
    }

    #endregion
}