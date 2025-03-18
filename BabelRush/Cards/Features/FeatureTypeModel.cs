using System.Collections.Generic;

using BabelRush.Data;

using Tomlyn.Syntax;

namespace BabelRush.Cards.Features;

[ModelSet("Feature")]
internal partial class FeatureTypeModel : IDataModel<FeatureType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    public string? Icon { get; set; }

    public FeatureType Convert()
    {
        Icon ??= Id;
        return new FeatureType(Id, Icon);
    }

    public static IReadOnlyCollection<IModel<FeatureType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, FeatureType>(source, out errorMessages);
}