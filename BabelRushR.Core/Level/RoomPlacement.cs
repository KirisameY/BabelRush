namespace BabelRushR.Core.Level;

public readonly record struct RoomPlacement(LevelNode Node, double Start)
{
    public double Length => Node.Definition.Length;

    public double End => Start + Length;
}
