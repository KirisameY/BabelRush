using System.Collections.Generic;

using BabelRush.Action;
using BabelRush.Mob;

namespace BabelRush.Card;

public interface ICard
{
    ICardType Type { get; }
    IList<IAction> Actions { get; }

    void Use(IEnumerable<IMob> targets);
}