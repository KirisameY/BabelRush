using System.Collections.Generic;
using System.Threading.Tasks;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Mobs;

namespace BabelRush.Cards;

public abstract class Card
{
    public abstract CardType Type { get; }
    public abstract int Cost { get; }
    public abstract IList<ActionInstance> Actions { get; }
    public abstract IList<Feature> Features { get; }
    public abstract bool TargetSelected();
    public abstract ValueTask<bool> Use(Mob user);

    public static Card Default { get; } = new CommonCard(CardType.Default);
}