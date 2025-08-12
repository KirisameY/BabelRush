using BabelRush.Data;

namespace BabelRush.Level.Stages.Template.GenerationRules;

[Model]
public partial class SeparationRuleModel
{
    [NecessaryProperty]
    public partial int From { get; set; }

    [NecessaryProperty]
    public partial int To { get; set; }

    [NecessaryProperty]
    public partial int SeparateBefore { get; set; }

    public SeparationRule Convert()
    {
        return new(From, To, SeparateBefore);
    }
}