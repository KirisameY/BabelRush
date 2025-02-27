using BabelRush.Data;
using BabelRush.Registers;

namespace BabelRush.Mobs.Actions;

[Model]
public partial class MobActionTemplateModel
{
    [NecessaryProperty]
    public partial string Action { get; set; }
    public int Value { get; set; } = -1;
    [NecessaryProperty]
    public partial double Time { get; set; }
    public double Weight { get; set; } = 1;
    public string? ConvertState { get; set; }


    public MobActionTemplate Convert() => new(ActionRegisters.Actions[Action], Value, Time, Weight, ConvertState);
}