using System.Linq;

using BabelRush.Cards;
using BabelRush.Gui.Cards;

using Godot;

using KirisameLib.Extensions;

using Tomlyn;

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

        var cardTypes = cardTypeModels.Select(m => m.Convert(Project.NameSpace, "cards"));
        //CardTypeData.FromTomlTable(Toml.ToModel(cardTypesFile.GetAsText()));
        var n = 0;
        foreach (var (_, cardType) in cardTypes)
        {
            GD.Print($"Card.{n}");
            var card = CardInterface.GetInstance(cardType.NewInstance());
            AddChild(card);
            card.Position = new(80 + 64 * n, 200);
            n++;
        }
    }
}