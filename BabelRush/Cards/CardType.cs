using System.Collections.Generic;
using System.Collections.Immutable;

using BabelRush.Actions;
using BabelRush.Cards.Features;
using BabelRush.Data;

using Godot;

namespace BabelRush.Cards;

public class CardType(string id, bool usable, int cost, IEnumerable<ActionType> actions, IEnumerable<FeatureType> features)
{
    public string Id { get; } = id;
    public NameDesc NameDesc => Registers.CardRegisters.CardNameDesc.GetItem(Id);
    public Texture2D Icon => Registers.CardRegisters.CardIcon.GetItem(Id);
    public bool Usable { get; } = usable;
    public int Cost { get; } = cost;
    public IImmutableList<ActionType> Actions { get; } = actions.ToImmutableList();
    public IImmutableList<FeatureType> Features { get; } = features.ToImmutableList();
    public Card NewInstance() => new CommonCard(this);

    public static CardType Default { get; } = new CardType("default", false, 0, [], []);
}