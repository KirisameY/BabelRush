using System.Collections.Generic;

using BabelRush.Mobs;

namespace BabelRush.Scenery;

public class Room(int length)
{
    //Properties
    public int Position { get; set; }
    public int Length { get; } = length;


    //Content
    public List<(MobType, Alignment, int)> Mobs { get; } = [];
}