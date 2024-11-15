using Godot;

namespace BabelRush.Tests.Cards;

public partial class CardTest : Node2D
{
    public override void _Ready()
    {
        CallDeferred(MethodName.Test);
    }

    public void Test()
    {
        //todo:要重构，这里先注释掉了
        // var cardTypesFile = FileAccess.Open("res://Tests/Cards/CardTypeList.toml", FileAccess.ModeFlags.Read);
        // var cardTypes = ((TomlTableArray)Toml.ToModel(cardTypesFile.GetAsText())["cards"]).Select(CardTypeModel.FromEntry);
        // //CardTypeData.FromTomlTable(Toml.ToModel(cardTypesFile.GetAsText()));
        // var n = 0;
        // foreach (var result in cardTypes)
        // {
        //     GD.Print($"Card.{n}");
        //     var card = CardInterface.GetInstance(result.Convert().NewInstance());
        //     AddChild(card);
        //     card.Position = new(80 + 64 * n, 200);
        //     n++;
        // }
    }
}