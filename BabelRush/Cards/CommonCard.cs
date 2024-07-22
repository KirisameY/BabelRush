using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public class CommonCard(CardType type) : Card
{
    public override CardType Type { get; } = type;
    public override int Cost => Type.Usable ? Type.Cost : -1;
    public override IList<Action> Actions { get; } = type.Actions.Select(actionType => actionType.NewInstance()).ToList();
    public override IList<Feature> Features { get; } = type.Features.Select(featureType => featureType.NewInstance()).ToList();

    public override void Use(Mob user, IReadOnlySet<Mob> targets)
    {
        foreach (var action in Actions)
        {
            action.Act(user, targets);
        }
    }
}