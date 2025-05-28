using BabelRush.Data;

namespace BabelRush.Scenery.Stages.Template;

[Model]
public partial class ConstantStageTemplateModel : StageTemplateModel
{
    public override (RegKey, StageTemplate) Convert(string nameSpace, string path)
    {
        RegKey id = (nameSpace, Id);
        return (id, new ConstantStageTemplate(id, StageNode.Default)); //todo
    }
}