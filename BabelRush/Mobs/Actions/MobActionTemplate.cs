using BabelRush.Actions;

namespace BabelRush.Mobs.Actions;

public class MobActionTemplate(ActionType actionType, int value, double time)
{
    public ActionType ActionType => actionType;
    public int Value => value;
    public double Time => time;

    public MobAction NewInstance(Mob mob) => new(mob, actionType.NewInstance(value), time);
}