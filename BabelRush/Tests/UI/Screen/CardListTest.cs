using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Cards;
using BabelRush.Gui.Screens.Cards;

using Godot;

namespace BabelRush.Tests.UI.Screen;

public partial class CardListTest : Node
{
    //Sub Nodes
    [field: AllowNull, MaybeNull]
    private CardListScreen CardListScreen => field ??= GetNode<CardListScreen>("CardListScreen");


    public override void _Ready()
    {
        CardListScreen.AddRange(Enumerable.Repeat<Card>(Card.Default, 5));
    }
}