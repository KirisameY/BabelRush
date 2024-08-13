using BabelRush.Cards;

using Godot;

using Tomlyn;
using Tomlyn.Model;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Tests.Card;

public partial class CardTest : Node2D
{
    public override void _Ready()
    {
        CallDeferred(MethodName.Test);
    }

    public void Test()
    {
        var cardTypesFile = FileAccess.Open("res://Tests/Card/CardTypeList.toml", FileAccess.ModeFlags.Read);
        var cardTypes = CardTypeData.FromTomlTable(Toml.ToModel(cardTypesFile.GetAsText()));
        var n = 0;
        foreach (var cardType in cardTypes)
        {
            GD.Print($"Card.{n}");
            var card = CardInterface.GetInstance(cardType.ToCardType().NewInstance());
            AddChild(card);
            card.Position = new(80 + 64 * n, 200);
            n++;
        }
    }
}