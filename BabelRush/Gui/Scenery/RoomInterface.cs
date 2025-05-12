using Godot;

namespace BabelRush.Gui.Scenery;

public partial class RoomInterface : Node2D //todo: 这货删掉吧，既然已经要改多层了，整出来东西大不了存个列表
{
    //Getter
    private RoomInterface() { }
    private static PackedScene Scene { get; } = ResourceLoader.Load<PackedScene>("res://Gui/Scenery/Room.tscn");

    public static RoomInterface GetInstance(int position, int size)
    {
        var instance = Scene.Instantiate<RoomInterface>();
        instance.Position = new(position, 0);
        instance.Size = size;
        return instance;
    }


    //Properties
    // temp display
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