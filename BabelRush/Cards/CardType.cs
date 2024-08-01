using System.Collections.Immutable;

using BabelRush.Actions;
using BabelRush.Cards.Features;

using Godot;

namespace BabelRush.Cards;

public abstract class CardType
{
    public abstract string Id { get; }
    public abstract string Name { get; }
    public abstract string Description { get; }
    public abstract Texture2D Icon { get; }
    public abstract bool Usable { get; }
    public abstract int Cost { get; }
    public abstract IImmutableList<ActionType> Actions { get; }
    public abstract IImmutableList<FeatureType> Features { get; }
    public abstract Card NewInstance();

    public static CardType Default { get; } = new CommonCardType("default", false, 0, [], []);
}