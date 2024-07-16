using BabelRush.Cards;

using Godot;

namespace BabelRush.Tests.Card;

public partial class CardTest : Node2D
{
    public override void _Ready()
    {
        CallDeferred(MethodName.Test);
    }

    public void Test()
    {
        var cardTypeList = ResourceLoader.Load<CardTypeResourceList>("res://Tests/Card/CardTypeList.tres");
        var n = 0;
        foreach (var cardType in cardTypeList)
        {
            var card = CardInterface.CreateInstance(cardType.ToCardType().NewInstance());
            AddChild(card);
            card.Position = new(80 + 64 * n, 200);
            n++;
        }
    }
}