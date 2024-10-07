using System.Collections.Generic;

using BabelRush.Data;

namespace BabelRush.Cards.Features;

public record FeatureTypeData(string Id) : IData<FeatureType, FeatureTypeData>
{
    public FeatureType ToModel() => new(Id);

    public static FeatureTypeData FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        return new(id);
    }
}