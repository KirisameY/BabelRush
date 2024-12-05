using System.Diagnostics.CodeAnalysis;
using System.Linq;

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

        CardListScreen.AddRange(Enumerable.Repeat(Card.Default, 5));
    }
}