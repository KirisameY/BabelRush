using BabelRush.Actions;

namespace BabelRush.Mobs.Actions;

public class MobActionTemplate(ActionType actionType, int value, double time, double weight = 1, string? convertState = null)
{
    public ActionType ActionType => actionType;
    public int Value => value;
    public double Time => time;
    public double Weight => weight;
    public string? ConvertState => convertState;

    public MobAction NewInstance(Mob mob) => new(mob, actionType.NewInstance(value), time);
}