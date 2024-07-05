using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public interface ICard
{
    ICardType Type { get; }
    IList<IAction> Actions { get; }

    void Use(IEnumerable<IMob> targets);
}