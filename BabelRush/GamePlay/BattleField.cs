using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Mobs;

using JetBrains.Annotations;

using KirisameLib.Core.Collections;
using KirisameLib.Core.Events;
using KirisameLib.Core.Logging;

namespace BabelRush.GamePlay;

[EventHandler]
public class BattleField(Mob player)
{
    //MobLists
    public Mob Player { get; } = player;

    private readonly List<Mob> _enemies = [];
    public IReadOnlyList<Mob> Enemies => _enemies.AsReadOnly();

    private readonly List<Mob> _friends = [player];
    public IReadOnlyList<Mob> Friends => _friends.AsReadOnly();

    private readonly List<Mob> _neutrals = [];
    public IReadOnlyList<Mob> Neutrals => _neutrals.AsReadOnly();

    private IReadOnlyList<Mob>? _allMobs;
    public IReadOnlyList<Mob> AllMobs => _allMobs ??= new CombinedListView<Mob>(Friends, Enemies);

    private IReadOnlyList<Mob>? _allMobsWithNeutral;
    public IReadOnlyList<Mob> AllMobsWithNeutral => _allMobsWithNeutral ??= new CombinedListView<Mob>(Friends, Enemies, Neutrals);

    private bool TryAddToList(Mob mob, Alignment alignment)
    {
        if (!TryGetList(alignment, out var list)) return false;
        if (list.Contains(mob)) return false;

        list.Add(mob);
        if (alignment == Alignment.Enemy)
        {
            if (list.Count == 1)
                EventBus.Publish(new BattleStartEvent());
        }
        return true;
    }

    private bool TryAddToList(Mob mob) => TryAddToList(mob, mob.Alignment);

    private bool TryRemoveFromList(Mob mob, Alignment alignment)
    {
        if (!TryGetList(alignment, out var list)) return false;
        if (!list.Remove(mob)) return false;

        if (alignment == Alignment.Enemy)
        {
            if (list.Count == 0)
                EventBus.Publish(new BattleEndEvent());
        }

        return true;
    }

    private bool TryRemoveFromList(Mob mob) => TryRemoveFromList(mob, mob.Alignment);

    private bool TryGetList(Alignment alignment, [NotNullWhen(true)] out List<Mob>? list)
    {
        switch (alignment)
        {
            case Alignment.Neutral:
                list = _neutrals;
                return true;
            case Alignment.Friend:
                list = _friends;
                return true;
            case Alignment.Enemy:
                list = _enemies;
                return true;
            default:
                Logger.Log(LogLevel.Error, nameof(TryGetList), $"Unexpected alignment: {alignment}");
                list = [];
                return false;
        }
    }


    //State
    public bool InBattle => _enemies.Count > 0;


    //Methods
    public void AddMob(Mob mob)
    {
        if (!TryAddToList(mob)) return;

        Logger.Log(LogLevel.Info, nameof(AddMob), $"Added mob {mob}");
        EventBus.Publish(new MobAddedEvent(mob));
    }

    public void AddMobs(IEnumerable<Mob> mobs)
    {
        foreach (var mob in mobs)
        {
            AddMob(mob);
        }
    }

    public void AddMobs(params Mob[] mobs) => AddMobs((IEnumerable<Mob>)mobs);

    public bool RemoveMob(Mob mob)
    {
        if (mob == Player)
        {
            Logger.Log(LogLevel.Warning, nameof(RemoveMob), "Unexpected attempt to remove player");
            return false;
        }

        if (!TryRemoveFromList(mob)) return false;

        Logger.Log(LogLevel.Info, nameof(RemoveMob), $"Removed mob {mob}");
        EventBus.Publish(new MobRemovedEvent(mob));
        return true;
    }


    //EventHandlers
    [EventHandler] [UsedImplicitly]
    private static void OnMobAlignmentChanged(MobAlignmentChangedEvent e)
    {
        var state = Play.BattleField;
        if (!state.AllMobsWithNeutral.Contains(e.Mob) || state.Player == e.Mob) return;
        state.TryRemoveFromList(e.Mob, e.OldAlignment);
        state.TryAddToList(e.Mob, e.NewAlignment);

        EventBus.Publish(new AlignmentUpdatedEvent());
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(BattleField));
}