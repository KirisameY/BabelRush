using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Gui.Cards;
using BabelRush.Gui.Utils;

using Godot;

using KirisameLib.Extensions;
using KirisameLib.Event;
using KirisameLib.Godot.Extensions;

namespace BabelRush.Gui.Screens.Cards;

[EventHandlerContainer]
internal partial class CardList : Control
{
    // Sub nodes & fields
    [field: AllowNull, MaybeNull]
    private GridContainer Container => field ??= GetNode<GridContainer>("MarginContainer/GridContainer");

    private readonly Dictionary<CardInterface, CardControl> _cardDict = new();


    // Public
    public event Action<CardInterface?>? CardSelected;

    public void Add(CardInterface ci)
    {
        if (_cardDict.ContainsKey(ci)) return;

        ci.Selectable = true;
        var cc = _cardDict[ci] = new CardControl(ci);
        Container.AddChild(cc);
    }

    public void AddRange(IEnumerable<CardInterface> cards) => cards.ForEach(Add);

    public void Remove(CardInterface ci)
    {
        if (!_cardDict.Remove(ci, out var cc)) return;

        Container.RemoveChild(cc);
    }

    public void Clear()
    {
        _cardDict.Clear();
        Container.RemoveChildren();
    }


    // Event Handlers
    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.GameEventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.GameEventBus);
    }

    [EventHandler]
    private void OnCardInterfaceSelected(CardInterfaceSelectedEvent e)
    {
        var ci = e.CardInterface;
        if (!_cardDict.ContainsKey(ci)) return;

        //todo: temp visual effect, to be replaced
        ci.YPosTween?.Kill();
        var tween = ci.YPosTween = CreateTween();
        tween.TweenProperty(ci, NodePaths.PositionY, e.Selected ? 34 : 36, 0.1f);
    }

    [EventHandler]
    private void OnCardInterfaceClicked(CardInterfaceClickedEvent e)
    {
        var ci = e.CardInterface;
        if (!_cardDict.ContainsKey(ci)) return;

        CardSelected?.Invoke(ci);
    }
}