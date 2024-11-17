using System;
using System.Linq;

using BabelRush.Cards;
using BabelRush.Data;
using BabelRush.Gui.Cards;

using Godot;

using KirisameLib.Core.Extensions;

using Tomlyn;
using Tomlyn.Model;

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
        // var cardTypes = ((TomlTableArray)Toml.ToModel(cardTypesFile.GetAsText())["cards"]).Select(CardTypeModel.FromEntry);
        var cardTypeModels = CardTypeModel.FromSource(Toml.Parse(cardTypesFile.GetAsText()), out var errors);
        errors.Messages.ForEach(GD.Print);

        var cardTypes = cardTypeModels.Select(m => m.Convert());
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