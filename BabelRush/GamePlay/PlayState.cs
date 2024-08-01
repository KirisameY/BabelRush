using System.Collections.Generic;
using System.Collections.Immutable;

using BabelRush.Mobs;

using KirisameLib.Collections;

namespace BabelRush.GamePlay;

public class PlayState(Mob player)
{
    //Member
    public Mob Player { get; } = player;

    private readonly List<Mob> _enemies = [];
    public IReadOnlyList<Mob> Enemies => _enemies.AsReadOnly();

    private readonly List<Mob> _friends = [player];
    public IReadOnlyList<Mob> Friends => _friends.AsReadOnly();

    private IReadOnlyList<Mob>? _allMobs;
    public IReadOnlyList<Mob> AllMobs => _allMobs ??= new CombinedListView<Mob>(Friends, Enemies);


    //Temp
    public void AddEnemy(Mob enemy) => _enemies.Add(enemy);
    public void AddFriend(Mob enemy) => _friends.Add(enemy);
}