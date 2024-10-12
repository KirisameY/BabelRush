namespace BabelRush.Cards.Features;

public class Feature(FeatureType type)
{
    public FeatureType Type { get; } = type;
    public int Value { get; set; }

    public static Feature Default { get; } = new(FeatureType.Default);
}