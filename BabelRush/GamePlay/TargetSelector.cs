using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards;
using BabelRush.Gui.Mobs;
using BabelRush.Mobs;

using JetBrains.Annotations;

using KirisameLib.Core.Events;

namespace BabelRush.GamePlay;

[EventHandler]
public static class TargetSelector
{
    //Members
    private static TargetRange _cursorSelectRange;
    private static TargetRange CursorSelectRange
    {
        get => _cursorSelectRange;
        set
        {
            if (CursorSelectRange == value) return;
            _cursorSelectRange = value;
            if (!GetRange(value).Contains(CursorSelected)) CursorSelected = null;
        }
    }

    private static TargetRange _autoSelectRange;
    private static TargetRange AutoSelectRange
    {
        get => _autoSelectRange;
        set
        {
            if (AutoSelectRange == value) return;
            _autoSelectRange = value;
            AutoSelected = GetRange(value);
        }
    }

    private static Mob? _cursorSelected;
    private static Mob? CursorSelected
    {
        get => _cursorSelected;
        set
        {
            if (value == CursorSelected) return;
            if (value is not null && !GetRange(CursorSelectRange).Contains(value)) return;
            var old = CursorSelected;
            _cursorSelected = value;
            if (old is not null)
                EventBus.Publish(new MobSelectedEvent(old, true, false));
            if (value is not null)
                EventBus.Publish(new MobSelectedEvent(value, true, true));
        }
    }

    private static IReadOnlyList<Mob> _autoSelected = [];
    private static IReadOnlyList<Mob> AutoSelected
    {
        get => _autoSelected;
        set
        {
            var old = AutoSelected;
            _autoSelected = value;
            var unite = old.Intersect(value).ToList();
            if (SelectPlayer) unite.Add(Play.BattleField.Player);
            foreach (var mob in old.Except(unite))
                EventBus.Publish(new MobSelectedEvent(mob, false, false));
            foreach (var mob in value.Except(unite))
                EventBus.Publish(new MobSelectedEvent(mob, false, true));
        }
    }

    private static bool _selectPlayer;
    private static bool SelectPlayer
    {
        get => _selectPlayer;
        set
        {
            if (SelectPlayer == value) return;
            _selectPlayer = value;
            if (!AutoSelected.Contains(Play.BattleField.Player))
                EventBus.Publish(new MobSelectedEvent(Play.BattleField.Player, false, value));
        }
    }


    //Methods
    public static IReadOnlyList<Mob> GetRange(TargetRange range) => range switch
    {
        TargetRange.Friend => Play.BattleField.Friends,
        TargetRange.Enemy  => Play.BattleField.Enemies,
        TargetRange.All    => Play.BattleField.AllMobs,
        0                  => [],
        _                  => throw new ArgumentOutOfRangeException(nameof(range), range, null)
    };

    public static IReadOnlyList<Mob> GetTargets(TargetPattern pattern) => pattern switch
    {
        TargetPattern.None    => [],
        TargetPattern.Self    => [Play.BattleField.Player],
        TargetPattern.Any any => CursorSelected is not null && GetRange(any.Range).Contains(CursorSelected) ? [CursorSelected] : [],
        TargetPattern.All all => GetRange(all.Range),
        _                     => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null)
    };


    //EventHandlers
    [EventHandler] [UsedImplicitly]
    private static void OnMobInterfaceSelected(MobInterfaceSelectedEvent e)
    {
        if (e.Selected) CursorSelected = e.Interface.Mob;
        else if (e.Interface.Mob == CursorSelected) CursorSelected = null;
    }

    [EventHandler] [UsedImplicitly]
    private static void OnCardPicked(CardPickedEvent e)
    {
        CursorSelectRange = 0;
        AutoSelectRange = 0;
        SelectPlayer = false;

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

    [EventHandler] [UsedImplicitly]
    private static void OnMobListChanged(MobListChangedEvent e)
    {
        //取巧的刷新方法，但是够用，也许以后会重写
        var temp = AutoSelected;
        AutoSelected = [];
        AutoSelected = temp;
    }
}