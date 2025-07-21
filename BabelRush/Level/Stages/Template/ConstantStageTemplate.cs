using BabelRush.Data;

namespace BabelRush.Level.Stages.Template;

public class ConstantStageTemplate(RegKey id, StageNode startNode) : StageTemplate(id)
{
    public StageNode StartNode => startNode;

    public override Stage GenerateStage()
    {
        return new(this, StartNode);
    }
}