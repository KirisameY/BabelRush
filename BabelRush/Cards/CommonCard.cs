using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.GamePlay;
using BabelRush.Mobs;

using KirisameLib.Events;

namespace BabelRush.Cards;

public class CommonCard(CardType type) : Card
{
    public override CardType Type { get; } = type;
    public override int Cost => Type.Usable ? Type.Cost : -1;
    public override IList<Action> Actions { get; } = type.Actions.Select(actionType => actionType.NewInstance()).ToList();
    public override IList<Feature> Features { get; } = type.Features.Select(featureType => featureType.NewInstance()).ToList();

    public override bool TargetSelected()
    {
        return Actions.All
            (action =>
                 action.Type.TargetPattern == TargetPattern.None ||
                 TargetSelector.GetTargets(action.Type.TargetPattern).Count > 0
            );
    }

    public override bool Use(Mob user)
    {
        if (!TargetSelected()) return false;

        EventBus.Publish(new BeforeCardUseEvent(this));
        foreach (var action in Actions)
        {
            action.Act(user, TargetSelector.GetTargets(action.Type.TargetPattern));
        }

        EventBus.Publish(new CardUsedEvent(this));
        return true;
    }
}