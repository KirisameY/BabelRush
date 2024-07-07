using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public class CommonCard(ICardType type) : ICard
{
    public ICardType Type { get; } = type;
    public IList<IAction> Actions { get; } = type.Actions.Select(actionType => actionType.NewInstance()).ToList();

    public void Use(IMob user, IReadOnlySet<IMob> targets)
    {
        foreach (var action in Actions)
        {
            action.Act(user, targets);
        }
    }
}