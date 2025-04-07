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

    public (RegKey, FeatureType) Convert(string nameSpace, string path)
    {
        RegKey id = (nameSpace, Id);
        RegKey icon = Icon?.WithDefaultNameSpace(nameSpace) ?? id;
        var feature = new FeatureType(id, icon);
        return (id, feature);
    }

    public static IReadOnlyCollection<IModel<FeatureType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, FeatureType>(source, out errorMessages);
}