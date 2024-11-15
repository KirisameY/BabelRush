using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.Model;

using Tomlyn.Syntax;

namespace BabelRush.Cards.Features;

[ModelSet("Feature")]
internal partial class FeatureTypeModel : IDataModel<FeatureType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }

    public FeatureType Convert()
    {
        return new FeatureType(Id);
    }

    public static IReadOnlyCollection<IModel<FeatureType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        IDataModel<FeatureType>.ParseFromSource<ModelSet>(source, out errorMessages);
}