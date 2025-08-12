using BabelRush.Data;

using KirisameLib.Randomization;

namespace BabelRush.Level.Stages.Template;

public abstract class StageTemplate(RegKey id)
{
    public RegKey Id => id;


    public abstract Stage GenerateStage(RandomBelt random);

    public static StageTemplate Default => new ConstantStageTemplate(RegKey.Default, StageNode.Default);
}