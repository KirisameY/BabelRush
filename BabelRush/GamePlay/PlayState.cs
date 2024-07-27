using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.GamePlay;

public class PlayState(Mob player)
{
    //Member
    public Mob Player { get; } = player;
    public List<Mob> Enemies { get; } = [];
}