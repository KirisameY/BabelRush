using Godot;

namespace BabelRush.Cards.Features;

public class CommonFeatureType(string id) : FeatureType
{
    public override string Id { get; } = id;
    public override string Name => Registers.FeatureName.GetItem(id);
    public override string Description => Registers.FeatureDesc.GetItem(id);
    public override Texture2D Icon => Registers.FeatureIcon.GetItem(id);

    public override Feature NewInstance()
    {
        var result = new CommonFeature(this);
        return result;
    }
}