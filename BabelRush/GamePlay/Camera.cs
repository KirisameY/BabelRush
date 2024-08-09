using Godot;

namespace BabelRush.GamePlay;

public partial class Camera : Camera2D
{
    //Following
    public float TargetPositionX { get; set; }

    private const float Smoothness = 0.9f;

    public override void _Process(double delta)
    {
        Position = Position.Lerp(new Vector2(TargetPositionX, 0), Smoothness);
    }
}