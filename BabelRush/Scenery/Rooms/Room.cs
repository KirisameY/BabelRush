namespace BabelRush.Scenery.Rooms;

public class Room(int length) // 这个类目前来看其实没用，如果以后还没用就删掉好了
{
    //Properties
    public int Position { get; set; }
    public int Length { get; } = length;
}