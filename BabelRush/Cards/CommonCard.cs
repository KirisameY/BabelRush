using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public class CommonCard(ICardType type) : ICard
{
    public ICardType Type { get; } = type;
    public int Cost => Type.Usable ? Type.Cost : -1;
    public IList<IAction> Actions { get; } = type.Actions.Select(actionType => actionType.NewInstance()).ToList();
    public IList<IFeature> Features { get; } = type.Features.Select(featureType => featureType.NewInstance()).ToList();

    public void Use(IMob user, IReadOnlySet<IMob> targets)
    {
        foreach (var action in Actions)
        {
            action.Act(user, targets);
        }
    }

    public static CommonCard Default { get; } = new(CommonCardType.Default);
}