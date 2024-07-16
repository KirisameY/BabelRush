using System.Collections.Generic;

using BabelRush.Actions;
using BabelRush.Cards.Features;

using Godot;

namespace BabelRush.Cards;

public class CommonCardType(string id, bool usable, int cost, IReadOnlyList<IActionType> actions, IReadOnlyList<IFeatureType> features)
    : ICardType
{
    public string Id { get; } = id;
    public string Name => Registers.CardName.GetItem(Id);
    public string Description => Registers.CardDesc.GetItem(Id);
    public Texture2D Icon => Registers.CardIcon.GetItem(Id);
    public bool Usable { get; } = usable;
    public int Cost { get; } = cost;
    public IReadOnlyList<IActionType> Actions { get; } = actions;
    public IReadOnlyList<IFeatureType> Features { get; } = features;

    public ICard NewInstance()
    {
        //暂且考虑将有必要用特殊卡牌类的条件判断写在这儿
        return new CommonCard(this);
    }


    public static CommonCardType Default { get; } = new CommonCardType("default", false, 0, [], []);
}