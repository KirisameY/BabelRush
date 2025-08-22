using System.Collections.Generic;

using BabelRush.Data;

using Tomlyn.Syntax;

namespace BabelRush.Effects;

[ModelSet("Effect")] // todo: we need具体类型 >_<
internal abstract partial class EffectTypeModel : IDataModel<EffectType>
{
    [NecessaryProperty]
    public partial string Id { get; set; }

    public string? Icon { get; set; } = null;

    public string? Polarity { get; set; } = "None";


    public abstract (RegKey, EffectType) Convert(string nameSpace, string path);

    public static IReadOnlyCollection<IModel<EffectType>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages) =>
        ModelUtils.ParseFromSource<ModelSet, EffectType>(source, out errorMessages);
}