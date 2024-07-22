namespace BabelRush.Cards.Features;

public class CommonFeature(FeatureType type) : Feature
{
    public override FeatureType Type { get; } = type;
}