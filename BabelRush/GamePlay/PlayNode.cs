using System.Diagnostics.CodeAnalysis;

using Godot;

namespace BabelRush.GamePlay;

public partial class PlayNode : Node
{
    //Getter
    private PlayNode() { }

    [field: AllowNull, MaybeNull]
    private static PackedScene Scene => field ??= ResourceLoader.Load<PackedScene>("res://GamePlay/Play.tscn");

    public static PlayNode CreateInstance()
    {
        var result = Scene.Instantiate<PlayNode>();
        result.Name = "Play";
        return result;
    }


    //Members
    private Camera? _camera;
    public Camera Camera => _camera ??= GetNode<Camera>("Camera");


    //Logging
    // private static Logger Logger { get; } = Game.LogBus.GetLogger("PlayNode");
}