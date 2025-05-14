using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Mobs;

using KirisameLib.Collections;
using KirisameLib.Event;
using KirisameLib.Logging;

namespace BabelRush.GamePlay;

[EventHandlerContainer]
public partial class BattleField(Mob player)
{
    //MobLists
    public Mob Player { get; } = player;

    private readonly List<Mob> _enemies = [];
    public IReadOnlyList<Mob> Enemies => _enemies.AsReadOnly();

    private readonly List<Mob> _friends = [player];
    public IReadOnlyList<Mob> Friends => _friends.AsReadOnly();

    private readonly List<Mob> _neutrals = [];
    public IReadOnlyList<Mob> Neutrals => _neutrals.AsReadOnly();

    [field: AllowNull, MaybeNull]
    public IReadOnlyList<Mob> AllMobs => field ??= new CombinedListView<Mob>(Friends, Enemies);

    [field: AllowNull, MaybeNull]
    public IReadOnlyList<Mob> AllMobsWithNeutral => field ??= new CombinedListView<Mob>(Friends, Enemies, Neutrals);

    private bool TryAddToList(Mob mob, Alignment alignment)
    {
        var list = GetList(alignment);
        if (list.Contains(mob)) return false;

        list.Add(mob);
        if (alignment == Alignment.Enemy)
        {
            if (list.Count == 1)
                Game.GameEventBus.Publish(new BattleStartEvent());
        }
        return true;
    }

    private bool TryAddToList(Mob mob) => TryAddToList(mob, mob.Alignment);

    private bool TryRemoveFromList(Mob mob, Alignment alignment)
    {
        var list = GetList(alignment);
        if (!list.Remove(mob)) return false;

        if (alignment == Alignment.Enemy)
        {
            if (list.Count == 0)
                Game.GameEventBus.Publish(new BattleEndEvent());
        }

        return true;
    }

    private bool TryRemoveFromList(Mob mob) => TryRemoveFromList(mob, mob.Alignment);

    private List<Mob> GetList(Alignment alignment)
    {
        switch (alignment)
        {
            case Alignment.Neutral: return _neutrals;
            case Alignment.Friend:  return _friends;
            case Alignment.Enemy:   return _enemies;
            default:
                Logger.Log(LogLevel.Error, nameof(GetList), $"Unexpected alignment: {alignment}");
                return [];
        }
    }


    //State
    public bool InBattle => _enemies.Count > 0;


    //Methods
    public IReadOnlyList<Mob> GetMobs(Alignment alignment) => GetList(alignment).AsReadOnly();

    public IReadOnlyList<Mob> GetOppositeMobs(Alignment alignment) => alignment switch
    {
        Alignment.Friend => Enemies,
        Alignment.Enemy  => Friends,
        _                => []
    };

    public void AddMob(Mob mob)
    {
        if (!TryAddToList(mob)) return;

        Logger.Log(LogLevel.Info, nameof(AddMob), $"Added mob {mob}");
        Game.GameEventBus.Publish(new InBattleMobAddedEvent(mob));
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
        Game.GameEventBus.Publish(new InBattleMobRemovedEvent(mob));
        return true;
    }


    //EventHandlers
    [EventHandler]
    private static void OnMobAlignmentChanged(MobAlignmentChangedEvent e)
    {
        var state = Game.Play!.BattleField;
        if (!state.AllMobsWithNeutral.Contains(e.Mob) || state.Player == e.Mob) return;
        state.TryRemoveFromList(e.Mob, e.OldAlignment);
        state.TryAddToList(e.Mob, e.NewAlignment);

        Game.GameEventBus.Publish(new AlignmentUpdatedEvent());
    }


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(BattleField));
}