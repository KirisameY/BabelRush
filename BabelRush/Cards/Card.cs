using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public abstract class Card
{
    public abstract CardType Type { get; }
    public abstract int Cost { get; }
    public abstract IList<Action> Actions { get; }
    public abstract IList<Feature> Features { get; }
    public abstract void Use(Mob user, IReadOnlyList<Mob> targets);

    public static Card Default { get; } = new CommonCard(CardType.Default);
}