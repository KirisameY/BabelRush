using System.Collections.Generic;

using BabelRush.Action;
using BabelRush.Entity;

namespace BabelRush.Card;

public interface ICard
{
    ICardType Type { get; }
    List<IAction> Actions { get; }

    void Use(IEnumerable<IEntity> targets);
}