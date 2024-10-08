using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.Parsing;

namespace BabelRush.Cards.Features;

public record FeatureTypeBox(string Id) : IDataBox<FeatureType, FeatureTypeBox>
{
    public FeatureType GetAsset() => new(Id);

    public static FeatureTypeBox FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        return new(id);
    }
}