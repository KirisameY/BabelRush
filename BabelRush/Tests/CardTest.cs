using BabelRush.Cards;

using Godot;

namespace BabelRush.Tests;

public partial class CardTest : Node2D
{
    private CardInterface Card => GetNode<CardInterface>("Card");

    public override void _Ready()
    {
        CallDeferred(MethodName.Test);
    }

    public void Test()
    {
        var cardTypeList = ResourceLoader.Load<CardTypeResourceList>("res://Tests/CardTypeList.tres");
        Card.Card = cardTypeList[1].ToCardType().NewInstance();
    }
}