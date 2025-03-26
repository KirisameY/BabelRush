using BabelRush.Actions;
using BabelRush.Data;
using BabelRush.Registers;

namespace BabelRush.Mobs.Actions;

public class MobActionTemplate(RegKey actionTypeId, int value, double time, double weight = 1, string? convertState = null)
{
    public ActionType ActionType => ActionRegisters.Actions[actionTypeId];
    public int Value => value;
    public double Time => time;
    public double Weight => weight;
    public string? ConvertState => convertState;

    public MobAction NewInstance(Mob mob) => new(mob, ActionType.NewInstance(value), time);
}