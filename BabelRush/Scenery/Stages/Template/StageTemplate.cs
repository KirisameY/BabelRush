using BabelRush.Data;

namespace BabelRush.Scenery.Stages.Template;

public abstract class StageTemplate(RegKey id)
{
    public RegKey Id => id;


    public abstract Stage GenerateStage();

    public static StageTemplate Default => new ConstantStageTemplate(RegKey.Default, StageNode.Default);
}