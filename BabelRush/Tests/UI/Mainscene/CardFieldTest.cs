using BabelRush.Cards;
using BabelRush.Gui.MainUI;

using Godot;

namespace BabelRush.Tests.UI.Mainscene;

public partial class CardFieldTest : Control
{
    private CardField? _cardField;
    private CardField CardField => _cardField ??= GetNode<CardField>("CardField");

    public void Test()
    {
        //Test
        void AddTestCard() => CardField.AddCard(CommonCard.Default);
        var tween = CreateTween();
        for (int i = 0; i < 8; i++)
        {
            tween.TweenCallback(Callable.From(AddTestCard)).SetDelay(0.1);
        }
    }
}