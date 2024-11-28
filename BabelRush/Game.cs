using Godot;

namespace BabelRush;

[GlobalClass]
public partial class Game : SceneTree
{
    public override void _Initialize()
    {
        base._Initialize();
        GD.Print("Game class loaded");
    }
}