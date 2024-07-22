namespace BabelRush.Cards.Features;

public abstract class Feature
{
    public abstract FeatureType Type { get; }
    
    public static CommonFeature Default { get; } = new(FeatureType.Default);
}