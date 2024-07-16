namespace BabelRush.Cards.Features;

public interface IFeature
{
    IFeatureType Type { get; }
}

public class CommonFeature(IFeatureType type) : IFeature
{
    public IFeatureType Type { get; } = type;

    public static CommonFeature Default { get; } = new(CommonFeatureType.Default);
}