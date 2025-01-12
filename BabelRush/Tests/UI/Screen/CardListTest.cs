using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Actions;
using BabelRush.Cards;
using BabelRush.Gui.Screens.Cards;

using Godot;

using KirisameLib.Event;

namespace BabelRush.Tests.UI.Screen;

public partial class CardListTest : Node
{
    //Sub Nodes
    [field: AllowNull, MaybeNull]
    private CardListScreen CardListScreen => field ??= GetNode<CardListScreen>("CardListScreen");


    public override void _Ready()
    {
        Game.EventBus.Subscribe<BaseEvent>(e => GD.Print(e));



        CardListScreen.Add(CardType.Default.NewInstance());
        CardListScreen.Add(CardType.Default.NewInstance());
        var testCardType = new CardType("testing", true, 2,
                                        [(new("test_action", new TargetPattern.Self(), []), 8)],
                                        [new("test_feature")]);
        CardListScreen.Add(testCardType.NewInstance());
    }
}