using BabelRush.Cards;
using BabelRush.Data;

using Godot;

using Tomlyn;

using CardInterface = BabelRush.Gui.Cards.CardInterface;

namespace BabelRush.Tests.Cards;

public partial class CardTest : Node2D
{
    public override void _Ready()
    {
        CallDeferred(MethodName.Test);
    }

    public void Test()
    {
        var cardTypesFile = FileAccess.Open("res://Tests/Card/CardTypeList.toml", FileAccess.ModeFlags.Read);
        var cardTypes = DataUtils.FromTomlTable<CardTypeData>(Toml.ToModel(cardTypesFile.GetAsText()), "cards");
        //CardTypeData.FromTomlTable(Toml.ToModel(cardTypesFile.GetAsText()));
        var n = 0;
        foreach (var result in cardTypes)
        {
            GD.Print($"Card.{n}");
            var card = CardInterface.GetInstance(result.Result.ToCardType().NewInstance());
            AddChild(card);
            card.Position = new(80 + 64 * n, 200);
            n++;
        }
    }
}