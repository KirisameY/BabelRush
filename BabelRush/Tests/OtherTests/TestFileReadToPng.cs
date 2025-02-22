using Godot;

namespace BabelRush.Tests.OtherTests;

public partial class TestFileReadToPng : Node2D
{
    // Called when the node enters the scene tree for the first time.
    public override async void _Ready()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //wait 1 frame

        var file = FileAccess.Open("res://meta_res/sprites/mob/healthbar_shield.png", FileAccess.ModeFlags.Read);
        var buffer = file.GetBuffer((long)file.GetLength());
        file.Close();

        var image = Image.CreateEmpty(1, 1, false, Image.Format.Rgba8);
        image.LoadPngFromBuffer(buffer);

        var tex = ImageTexture.CreateFromImage(image);

        ToDraw = tex;
        QueueRedraw();
    }

    private Texture2D ToDraw { get; set; } = new PlaceholderTexture2D { Size = new(16, 16) };

    public override void _Draw()
    {
        DrawTextureRect(ToDraw, new(0, 0, ToDraw.GetSize()), false);
        base._Draw();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta) { }
}