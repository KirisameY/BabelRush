using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.Model;

using Tomlyn.Syntax;

namespace BabelRush.Cards.Features;

public record FeatureTypeModel(string Id) : IDataModel<FeatureType>
{
    public FeatureType Convert() => new(Id);

    // public static FeatureTypeModel FromEntry(IDictionary<string, object> entry)
    // {
    //     var id = (string)entry["id"];
    //     return new(id);
    // }

    public static IReadOnlyCollection<IModel<FeatureType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages)
    {
        throw new System.NotImplementedException();
    }
}