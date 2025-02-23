using System.Threading.Tasks;

using BabelRush.Actions;
using BabelRush.GamePlay;

namespace BabelRush.Mobs.Actions;

public class MobAction(Mob mob, ActionInstance action, double time)
{
    #region Properties

    public Mob Mob => mob;
    public ActionInstance Action => action;
    public double Progress
    {
        get;
        set
        {
            field = value;
            if (field >= value) _ = Act();
        }
    }
    public double Time => time;
    public double ProgressRate => Progress / Time;

    #endregion


    #region Public Methods

    public async ValueTask<bool> Act()
    {
        var targets = Play.BattleField.GetOppositeMobs(Mob.Alignment);

        var request = await Game.EventBus.PublishAndWaitFor(new MobActionExecuteRequest(Mob, this, new()));
        if (request.Cancel.Canceled)
        {
            Game.EventBus.Publish(new MobActionCanceledEvent(Mob, this));
            return false;
        }

        Action.Act(Mob, targets);
        Game.EventBus.Publish(new MobActionExecutedEvent(Mob, this));
        return true;
    }

    #endregion
}