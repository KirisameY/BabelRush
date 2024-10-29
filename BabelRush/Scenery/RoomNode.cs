using Godot;

namespace BabelRush.Scenery;

public partial class RoomNode : Node2D
{
    //Getter
    private RoomNode() { }
    private static PackedScene Scene { get; } = ResourceLoader.Load<PackedScene>("res://Scenery/Room.tscn");

    public static RoomNode GetInstance(int position, int size)
    {
        var instance = Scene.Instantiate<RoomNode>();
        instance.Position = new(position, 0);
        instance.Size = size;
        return instance;
    }


    //Properties
    private Control? _endNode;
    private Control EndNode => _endNode ??= GetNode<Control>("End");

    private int _size;
    public int Size
    {
        get => _size;
        private set
        {
            _size = value;
            EndNode.Position = new(value, 0);
        }
    }
}