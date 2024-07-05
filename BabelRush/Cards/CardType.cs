using System.Collections.Generic;

using BabelRush.Actions;

using Godot;

namespace BabelRush.Cards;

public class CardType(string name, Texture2D icon, bool usable, int cost)
    : ICardType
{
    public string Name { get; } = name;
    public Texture2D Icon { get; } = icon;
    public bool Usable { get; } = usable;
    public int Cost { get; } = cost;
    public IReadOnlyList<IAction> Actions { get; } = [];

    public ICard NewInstance()
    {
        //暂且考虑将有必要用特殊卡牌类的条件判断写在这儿
        return new CommonCard(this);
    }
}