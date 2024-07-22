using Godot;

namespace BabelRush.Cards.Features;

public abstract class FeatureType
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Texture2D Icon { get; }
    public abstract Feature NewInstance();
    
    public static CommonFeatureType Default { get; } = new("default");
}