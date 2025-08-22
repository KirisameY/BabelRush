using System;
using System.Collections.Generic;

using BabelRush.Data;

using Tomlyn.Syntax;

namespace BabelRush.Effects;

[ModelSet<EmptyEffectTypeModel>("EmptyEffect")]
[ModelSet<ScriptEffectTypeModel>("ScriptEffect")]
internal abstract partial class EffectTypeModel : IDataModel<EffectType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }

    public string? Icon { get; set; } = null;

    public string Polarity { get; set; } = "None";


    public (RegKey, EffectType) Convert(string nameSpace, string path)
    {
        var id = (nameSpace, Id);
        var icon = Icon?.WithDefaultNameSpace(nameSpace) ?? id;
        var polarity = Enum.Parse<EffectPolarity>(Polarity);
        return Convert(nameSpace, id, icon, polarity);
    }

    protected abstract (RegKey, EffectType) Convert(string nameSpace, RegKey id, RegKey icon, EffectPolarity polarity);

    public static IReadOnlyCollection<IModel<EffectType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, EffectType>(source, out errorMessages);

    partial void CustomCheck(List<string> errorList)
    {
        if (Enum.TryParse<EffectPolarity>(Polarity, out _)) errorList.Add($"Unrecognized polarity: {Polarity}");
    }
}