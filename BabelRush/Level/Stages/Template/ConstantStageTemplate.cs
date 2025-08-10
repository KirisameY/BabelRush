using BabelRush.Data;

using KirisameLib.Randomization;

namespace BabelRush.Level.Stages.Template;

public class ConstantStageTemplate(RegKey id, StageNode startNode) : StageTemplate(id)
{
    public StageNode StartNode => startNode;

    public override Stage GenerateStage(RandomBelt random)
    {
        return new(this, StartNode);
    }
}