using System.Collections.Generic;

namespace BabelRush.Cards.Features;

public record FeatureTypeData(string Id)
{
    public FeatureType ToFeatureType() => new(Id);

    public static FeatureTypeData FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        return new(id);
    }
}