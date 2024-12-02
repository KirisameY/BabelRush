using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.GamePlay;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public class CommonCard(CardType type) : Card
{
    public override CardType Type { get; } = type;
    public override int Cost => Type.Usable ? Type.Cost : -1;
    public override IList<ActionInstance> Actions { get; } = type.Actions.Select(t => t.type.NewInstance(t.value)).ToList();
    public override IList<Feature> Features { get; } = type.Features.Select(featureType => featureType.NewInstance()).ToList();

    public override bool TargetSelected() =>
        Actions.Count > 0 && Actions.All
            (action =>
                 action.Type.TargetPattern is TargetPattern.None ||
                 TargetSelector.GetTargets(action.Type.TargetPattern).Count > 0
            );

    public override async ValueTask<bool> Use(Mob user)
    {
        if (!TargetSelected()) return false;

        foreach (var action in Actions)
        {
            action.Act(user, TargetSelector.GetTargets(action.Type.TargetPattern));
        }

        var canceled = (
            await Game.EventBus.PublishAndWaitFor(new BeforeCardUseEvent(this, new()))
        ).Cancel.Canceled;
        if (canceled) return false;

        var toExhause = (
            await Game.EventBus.PublishAndWaitFor(new CardUsedEvent(this, true, false))
        ).ToExhaust;

        var removed = toExhause
            ? await Play.CardHub.ExhaustCard(this)
            : await Play.CardHub.DiscardCard(this);

        return removed;
    }
}