using System.Collections.Generic;

using BabelRush.Data;

using Tomlyn.Syntax;

namespace BabelRush.Effects;

[ModelSet("Effect")]
internal partial class EffectTypeModel : IDataModel<EffectType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }

    public string? Icon { get; set; } = null;


    public (RegKey, EffectType) Convert(string nameSpace, string path)
    {
        var id = (nameSpace, Id);
        var icon = Icon?.WithDefaultNameSpace(nameSpace) ?? id;

        return (id, new(id, icon));
    }

    public static IReadOnlyCollection<IModel<EffectType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, EffectType>(source, out errorMessages);
}