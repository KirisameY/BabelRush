using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.GamePlay;
using BabelRush.Mobs;

using KirisameLib.Core.Events;

namespace BabelRush.Cards;

public class CommonCard(CardType type) : Card
{
    public override CardType Type { get; } = type;
    public override int Cost => Type.Usable ? Type.Cost : -1;
    public override IList<Action> Actions { get; } = type.Actions.Select(actionType => actionType.NewInstance()).ToList();
    public override IList<Feature> Features { get; } = type.Features.Select(featureType => featureType.NewInstance()).ToList();

    public override bool TargetSelected() =>
        Actions.Count > 0 && Actions.All
            (action =>
                 action.Type.TargetPattern is TargetPattern.None ||
                 TargetSelector.GetTargets(action.Type.TargetPattern).Count > 0
            );

    public override bool Use(Mob user)
    {
        if (!TargetSelected()) return false;

        var useCancel = new CancelToken();
        EventBus.Publish(new BeforeCardUseEvent(this, useCancel));
        if (useCancel.Canceled) return false;

        foreach (var action in Actions)
        {
            action.Act(user, TargetSelector.GetTargets(action.Type.TargetPattern));
        }

        var toExhause = new Variable<bool>(false);
        EventBus.Publish(new CardUsedEvent(this, toExhause));

        var removed = toExhause
                          ? Play.CardHub.ExhaustCard(this)
                          : Play.CardHub.DiscardCard(this);

        return removed;
    }
}