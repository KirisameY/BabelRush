using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Cards.Features;

using Godot;

namespace BabelRush.Cards;

public class CommonCardType(string id, bool usable, int cost, IReadOnlyList<ActionType> actions, IReadOnlyList<FeatureType> features)
    : CardType
{
    public override string Id { get; } = id;
    public override string Name => Registers.CardName.GetItem(Id);
    public override string Description => Registers.CardDesc.GetItem(Id);
    public override Texture2D Icon => Registers.CardIcon.GetItem(Id);
    public override bool Usable { get; } = usable;
    public override int Cost { get; } = cost;
    public override IReadOnlyList<ActionType> Actions { get; } = actions;
    public override IReadOnlyList<FeatureType> Features { get; } = features;

    public override Card NewInstance()
    {
        //暂且考虑将有必要用特殊卡牌类的条件判断写在这儿
        return new CommonCard(this);
    }
}