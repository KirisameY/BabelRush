using System.Diagnostics.CodeAnalysis;

using BabelRush.Gui.Screens.InGame;

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
    [field: AllowNull, MaybeNull]
    public Camera Camera => field ??= GetNode<Camera>("Camera");

    [field: AllowNull, MaybeNull]
    public MapScreen MapScreen => field ??= GetNode<MapScreen>("MapScreen");

    //Logging
    // private static Logger Logger { get; } = Game.LogBus.GetLogger("PlayNode");
}