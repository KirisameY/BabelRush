using BabelRush.Data;

namespace BabelRush.Scenery.Stages.Template;

public class ConstantStageTemplate(RegKey id, StageNode startNode) : StageTemplate(id)
{
    public StageNode StartNode => startNode;

    public override Stage GenerateStage()
    {
        return new(this, StartNode);
    }
}