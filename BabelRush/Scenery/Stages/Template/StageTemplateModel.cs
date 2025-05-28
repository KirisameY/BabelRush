using System.Collections.Generic;

using BabelRush.Data;

using Tomlyn.Syntax;

namespace BabelRush.Scenery.Stages.Template;

[ModelSet<ConstantStageTemplateModel>("ConstantStage")] // todo: multi type
public abstract partial class StageTemplateModel : IDataModel<StageTemplate>
{
    [NecessaryProperty]
    public partial string Id { get; set; }

    public abstract (RegKey, StageTemplate) Convert(string nameSpace, string path);

    public static IReadOnlyCollection<IModel<StageTemplate>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages)
    {
        return ModelUtils.ParseFromSource<ModelSet, StageTemplate>(source, out errorMessages);
    }
}