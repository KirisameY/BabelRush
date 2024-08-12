using BabelRush.Gui.MainUI;

using Godot;

namespace BabelRush.Tests.UI.Mainscene;

public partial class MainSceneUiTest : Control
{
    private CardField? _cardField;
    private CardField CardField => _cardField ??= GetNode<CardField>("MainUi/CardField");

    private static StringName StrNameSetValue { get; } = "SetValue";

    public override void _Ready()
    {
        // ReSharper disable once StringLiteralTypo
        Node apBall = GetNode("MainUi/ApBar/Apball");
        GetNode<HSlider>("HSlider").ValueChanged +=
            value => apBall.CallDeferred(StrNameSetValue,value);
    }

    public void Test()
    {
        //Test
        //void AddTestCard() => CardField.AddCard(Cards.Card.Default);
        
        // var tween = CreateTween();
        // for (int i = 0; i < 8; i++)
        // {
        //     tween.TweenCallback(Callable.From(AddTestCard)).SetDelay(0.1);
        // }
        //AddTestCard();
    }
}