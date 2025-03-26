using BabelRush.Data;

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


    public MobActionTemplate Convert(string nameSpace) => new(Action.WithDefaultNameSpace(nameSpace), Value, Time, Weight, ConvertState);
}