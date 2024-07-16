using System;
using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public interface ICard
{
    ICardType Type { get; }
    int Cost { get; }
    IList<IAction> Actions { get; }

    void Use(IMob user, IReadOnlySet<IMob> targets);
}