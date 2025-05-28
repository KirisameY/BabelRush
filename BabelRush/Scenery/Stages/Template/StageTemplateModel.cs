using System;
using System.Collections.Generic;

using BabelRush.Data;

using Tomlyn.Syntax;

namespace BabelRush.Scenery.Stages.Template;

[ModelSet("Stage")] // todo: multi type
public partial class StageTemplateModel : IDataModel<StageTemplate>
{
    [NecessaryProperty]
    public partial string Id { get; set; }

    public (RegKey, StageTemplate) Convert(string nameSpace, string path)
    {
        RegKey id = (nameSpace, Id);
        throw new NotImplementedException();
    }

    public static IReadOnlyCollection<IModel<StageTemplate>> FromSource(DocumentSyntax source, out ModelParseErrorInfo errorMessages)
    {
        return ModelUtils.ParseFromSource<ModelSet, StageTemplate>(source, out errorMessages);
    }
}