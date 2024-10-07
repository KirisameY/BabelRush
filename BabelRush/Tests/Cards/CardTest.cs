using System.Linq;

using BabelRush.Cards;

using Godot;

using Tomlyn;
using Tomlyn.Model;

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
        var cardTypesFile = FileAccess.Open("res://Tests/Cards/CardTypeList.toml", FileAccess.ModeFlags.Read);
        var cardTypes = ((TomlTableArray)Toml.ToModel(cardTypesFile.GetAsText())["cards"]).Select(CardTypeData.FromEntry);
        //CardTypeData.FromTomlTable(Toml.ToModel(cardTypesFile.GetAsText()));
        var n = 0;
        foreach (var result in cardTypes)
        {
            GD.Print($"Card.{n}");
            var card = CardInterface.GetInstance(result.ToModel().NewInstance());
            AddChild(card);
            card.Position = new(80 + 64 * n, 200);
            n++;
        }
    }
}