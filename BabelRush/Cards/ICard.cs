using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public interface ICard
{
    ICardType Type { get; }
    int Cost { get; }
    IList<IAction> Actions { get; }
    IList<IFeature> Features { get; }

    void Use(IMob user, IReadOnlySet<IMob> targets);
}