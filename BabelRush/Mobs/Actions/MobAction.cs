using BabelRush.Actions;

namespace BabelRush.Mobs.Actions;

public class MobAction(ActionInstance action, double time)
{
    public ActionInstance Action => action;
    public double Progress { get; set; }
    public double Time => time;
}