using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Gui.Mob;
using BabelRush.Mobs;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.GamePlay;

[EventHandler]
public static class TargetSelector
{
    //Members
    private static Mob? CursorSelected { get; set; }


    //Methods
    public static IReadOnlyList<Mob> GetTargets(TargetPattern pattern) => pattern switch
    {
        TargetPattern.None      => [],
        TargetPattern.Self      => [Play.State.Player],
        TargetPattern.Any       => CursorSelected is null ? [] : [CursorSelected],
        TargetPattern.AnyEnemy  => CursorSelected is not null && Play.State.Enemies.Contains(CursorSelected) ? [CursorSelected] : [],
        TargetPattern.AnyFriend => CursorSelected is not null && Play.State.Friends.Contains(CursorSelected) ? [CursorSelected] : [],
        TargetPattern.All       => Play.State.AllMobs,
        TargetPattern.AllEnemy  => Play.State.Enemies,
        TargetPattern.AllFriend => Play.State.Friends,
        _                       => throw new ArgumentOutOfRangeException(nameof(pattern), pattern, null)
    };


    //EventHandlers
    [EventHandler] [UsedImplicitly]
    private static void OnMobInterfaceSelected(MobInterfaceSelectedEvent e)
    {
        if (e.Selected) CursorSelected = e.Interface.Mob;
        else if (e.Interface.Mob == CursorSelected) CursorSelected = null;
    }
}