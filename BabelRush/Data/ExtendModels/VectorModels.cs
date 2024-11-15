using Godot;

namespace BabelRush.Data.ExtendModels;

public class Vector2Model
{
    public float X { get; set; }
    public float Y { get; set; }

    public static implicit operator Vector2(Vector2Model model) => new(model.X, model.Y);
}

public class Vector2IModel
{
    public int X { get; set; }
    public int Y { get; set; }

    public static implicit operator Vector2I(Vector2IModel model) => new(model.X, model.Y);
}