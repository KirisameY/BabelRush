using Godot;

namespace BabelRush.Scenery;

public abstract class VisualObject : SceneObject
{
    public abstract Node CreateInterface();

    public abstract float Parallax { get; }
}