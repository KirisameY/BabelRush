using System;
using System.Linq;

using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Gui.Cards;

using Godot;

using Tomlyn;
using Tomlyn.Model;

namespace BabelRush.Tests.Cards;

public partial class CardTest : Node2D
{
    public override async void _Ready()
    {
        //CallDeferred(MethodName.Test);
        // await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        // Test();
        IDataModel<CardType> model = new CardTypeModel { Id = "test" };
        GD.Print(model.Id);
    }

    int _frame = 0;

    public override void _Process(double delta)
    {
        _frame++;
        if (_frame == 2) Test();
    }

    public void Test()
    {
        var cardTypesFile = FileAccess.Open("res://Tests/Cards/CardTypeList.toml", FileAccess.ModeFlags.Read);
        // var cardTypes = ((TomlTableArray)Toml.ToModel(cardTypesFile.GetAsText())["cards"]).Select(CardTypeModel.FromEntry);
        var cardTypes = CardTypeModel.FromSource(Toml.Parse(cardTypesFile.GetAsText()), out _).Select(m => m.Convert());
        //CardTypeData.FromTomlTable(Toml.ToModel(cardTypesFile.GetAsText()));
        var n = 0;
        foreach (var cardType in cardTypes)
        {
            GD.Print($"Card.{n}");
            var card = CardInterface.GetInstance(cardType.NewInstance());
            AddChild(card);
            card.Position = new(80 + 64 * n, 200);
            n++;
        }
    }
}