using Godot;

namespace BabelRush.Cards.Features;

public interface IFeatureType
{
    string Id { get; }
    string Name { get; }
    string Description { get; }
    Texture2D Icon { get; }

    IFeature NewInstance();
}

public class CommonFeatureType(string id) : IFeatureType
{
    public string Id { get; } = id;
    public string Name => Registers.FeatureName.GetItem(id);
    public string Description => Registers.FeatureDesc.GetItem(id);
    public Texture2D Icon => Registers.FeatureIcon.GetItem(id);

    public IFeature NewInstance()
    {
        var result = new CommonFeature(this);
        return result;
    }

    public static CommonFeatureType Default { get; } = new("default");
}