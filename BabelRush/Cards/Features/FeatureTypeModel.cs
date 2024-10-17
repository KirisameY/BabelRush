using System.Collections.Generic;

using BabelRush.Data;

namespace BabelRush.Cards.Features;

public record FeatureTypeModel(string Id) : IDataModel<FeatureType>
{
    public FeatureType Convert() => new(Id);

    public static FeatureTypeModel FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        return new(id);
    }

    public static IModel<FeatureType> FromSource(IDictionary<string, object> source) => FromEntry(source);
}