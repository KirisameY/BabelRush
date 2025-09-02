using BabelRush.Actions;
using BabelRush.Utils;

using KirisameLib.Asynchronous.SyncTasking;
using KirisameLib.Logging;

namespace BabelRush.Mobs.Actions;

public partial class MobAction(Mob mob, ActionInstance action, double time)
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
            if (field >= value) _ = ActAsync();
        }
    }
    public double Time => time;
    public double ProgressRate => Progress / Time;

    #endregion


    #region Public Methods

    [LogToSync]
    public async SyncTask<bool> ActAsync()
    {
        var targets = Game.Play!.BattleField.GetOppositeMobs(Mob.Alignment);

        var request = await Game.GameEventBus.PublishAndWaitFor(new MobActionExecuteRequestEvent(Mob, this, new()));
        if (request.Cancel.Canceled)
        {
            Game.GameEventBus.Publish(new MobActionCanceledEvent(Mob, this));
            return false;
        }

        Action.Act(Mob, targets);
        Game.GameEventBus.Publish(new MobActionExecutedEvent(Mob, this));
        return true;
    }

    #endregion


    // Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(MobAction));
}