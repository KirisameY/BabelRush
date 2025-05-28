using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards;
using BabelRush.Gui.Mobs;
using BabelRush.Mobs;

using KirisameLib.Event;

namespace BabelRush.GamePlay;

[EventHandlerContainer]
public partial class TargetSelector
{
    //Members
    private static TargetRange CursorSelectRange
    {
        get;
        set
        {
            if (CursorSelectRange == value) return;
            field = value;
            if (!GetRange(value).Contains(CursorSelected)) CursorSelected = null;
        }
    }

    private static TargetRange AutoSelectRange
    {
        get;
        set
        {
            if (AutoSelectRange == value) return;
            field        = value;
            AutoSelected = GetRange(value);
        }
    }

    private static Mob? CursorSelected
    {
        get;
        set
        {
            if (value == CursorSelected) return;
            if (value is not null && !GetRange(CursorSelectRange).Contains(value)) return;
            var old = CursorSelected;
            field = value;
            if (old is not null)
                Game.GameEventBus.Publish(new MobSelectedEvent(old, true, false));
            if (value is not null)
                Game.GameEventBus.Publish(new MobSelectedEvent(value, true, true));
        }
    }

    private static IReadOnlyList<Mob> AutoSelected
    {
        get;
        set
        {
            var old = AutoSelected;
            field = value;
            var unite = old.Intersect(value).ToList();
            if (SelectPlayer) unite.Add(Game.Play!.BattleField.Player);
            foreach (var mob in old.Except(unite))
                Game.GameEventBus.Publish(new MobSelectedEvent(mob, false, false));
            foreach (var mob in value.Except(unite))
                Game.GameEventBus.Publish(new MobSelectedEvent(mob, false, true));
        }
    } = [];

    private static bool SelectPlayer
    {
        get;
        set
        {
            if (SelectPlayer == value) return;
            field = value;
            if (!AutoSelected.Contains(Game.Play!.BattleField.Player))
                Game.GameEventBus.Publish(new MobSelectedEvent(Game.Play.BattleField.Player, false, value));
        }
    }


    //Methods
    public static IReadOnlyList<Mob> GetRange(TargetRange range) => range switch
    {
        _ when Game.Play is null => [],
        TargetRange.Friend       => Game.Play.BattleField.Friends,
        TargetRange.Enemy        => Game.Play.BattleField.Enemies,
        TargetRange.All          => Game.Play.BattleField.AllMobs,
        0                        => [],
        _                        => throw new ArgumentOutOfRangeException(nameof(range), range, null)
    };

    public static IReadOnlyList<Mob> GetTargets(TargetPattern pattern) => pattern switch
    {
        _ when Game.Play is null => [],
        TargetPattern.None       => [],
        TargetPattern.Self       => [Game.Play.BattleField.Player],
        TargetPattern.Any any    => CursorSelected is not null && GetRange(any.Range).Contains(CursorSelected) ? [CursorSelected] : [],
        TargetPattern.All all    => GetRange(all.Range),
        _                        => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null)
    };


    //EventHandlers
    [EventHandler]
    private static void OnMobInterfaceSelected(MobInterfaceSelectedEvent e)
    {
        if (Game.Play is null) return;

        if (e.Selected) CursorSelected                             = e.Interface.Mob;
        else if (e.Interface.Mob == CursorSelected) CursorSelected = null;
    }

    [EventHandler]
    private static void OnCardPicked(CardPickedEvent e)
    {
        if (Game.Play is null) return;

        CursorSelectRange = 0;
        AutoSelectRange   = 0;
        SelectPlayer      = false;

        if (!e.Picked) return;

        //分组遍历卡牌的actions
        var card = e.Card;
        var group = card.Actions
                        .Select(action => action.Type.TargetPattern)
                        .GroupBy(pattern => pattern.GetType());
        foreach (var patterns in group)
        {
            //根据组的type
            switch (patterns.Key.Name)
            {
                case nameof(TargetPattern.None):
                    break;
                case nameof(TargetPattern.Self):
                    SelectPlayer = true;
                    break;
                case nameof(TargetPattern.Any):
                    SetAny(patterns);
                    break;
                case nameof(TargetPattern.All):
                    SetAll(patterns);
                    break;
            }
        }
        return;

        void SetAny(IGrouping<Type, TargetPattern> patterns)
        {
            var range = TargetRange.All;
            foreach (var pattern in patterns)
            {
                if (pattern is not TargetPattern.Any any) continue;
                range &= any.Range;
            }
            CursorSelectRange = range;
        }

        void SetAll(IGrouping<Type, TargetPattern> patterns)
        {
            var range = (TargetRange)0;
            foreach (var pattern in patterns)
            {
                if (pattern is not TargetPattern.All all) continue;
                range |= all.Range;
            }
            AutoSelectRange = range;
        }
    }

    [EventHandler]
    private static void OnInBattleMobListChanged(InBattleMobListChangedEvent e)
    {
        //取巧的刷新方法，但是够用，也许以后会重写
        var temp = AutoSelected;
        AutoSelected = [];
        AutoSelected = temp;
    }
}