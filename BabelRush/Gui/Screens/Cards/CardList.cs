using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Cards;
using BabelRush.Gui.Cards;

using Godot;

using KirisameLib.Core.Extensions;
using KirisameLib.Event;
using KirisameLib.Godot.Extensions;

namespace BabelRush.Gui.Screens.Cards;

[EventHandlerContainer]
public partial class CardList : Control
{
    // Sub nodes & fields
    [field: AllowNull, MaybeNull]
    private GridContainer Container => field ??= GetNode<GridContainer>("MarginContainer/GridContainer");

    private readonly HashSet<CardInterface> _cards = [];


    // Public
    public event Action<CardInterface?>? CardSelected;

    public void Add(CardInterface card)
    {
        if (!_cards.Add(card)) return;

        Container.AddChild(card);
    }

    public void AddRange(IEnumerable<CardInterface> cards) => cards.ForEach(Add);

    public void Remove(CardInterface card)
    {
        if (!_cards.Remove(card)) return;

        Container.RemoveChild(card);
    }

    public void Clear()
    {
        _cards.Clear();
        Container.RemoveChildren();
    }


    // Event Handlers
    public override void _EnterTree()
    {
        SubscribeInstanceHandler(Game.EventBus);
    }

    public override void _ExitTree()
    {
        UnsubscribeInstanceHandler(Game.EventBus);
    }

    [EventHandler]
    private void OnCardInterfaceSelected(CardInterfaceSelectedEvent e)
    {
        var ci = e.CardInterface;
        if (!_cards.Contains(ci)) return;

        ci.YPosTween?.Kill();
        var tween = ci.YPosTween = CreateTween();
        tween.TweenProperty(ci, CardInterface.NodePaths.PositionY, e.Selected ? -16 : 0, 0.1f);
    }

    [EventHandler]
    private void OnCardInterfaceClicked(CardInterfaceClickedEvent e)
    {
        var ci = e.CardInterface;
        if (!_cards.Contains(ci)) return;

        CardSelected?.Invoke(ci);
    }
}