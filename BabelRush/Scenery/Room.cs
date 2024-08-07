using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Scenery;

public class Room
{
    //Properties
    public int Position { get; set; }
    public int Length { get; }


    //Content
    public List<(MobType, int)> Mobs { get; } = [];
}