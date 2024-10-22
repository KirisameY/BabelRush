using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.Model;

namespace BabelRush.Cards.Features;

public record FeatureTypeModel(string Id) : IDataModel<FeatureType>
{
    public FeatureType Convert() => new(Id);

    public static FeatureTypeModel FromEntry(IDictionary<string, object> entry)
    {
        var id = (string)entry["id"];
        return new(id);
    }
    
    public static IModel<FeatureType>[] FromSource(byte[] source, out ModelParseErrorInfo errorMessages)
    {
        throw new System.NotImplementedException();
    }
}