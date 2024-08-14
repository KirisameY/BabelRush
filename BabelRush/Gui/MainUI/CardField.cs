using System.Collections.Generic;
using System.Linq;

using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Gui.Card;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control
{
    //Init
    public override void _Ready() { }


    //Member
    private static CardPile Pile => Play.State.PlayerInfo.CardField;
    private Dictionary<Cards.Card, CardInterface> CardDict { get; } = [];
    private IReadOnlyCollection<CardInterface> CardInterfaceList => CardDict.Values;
    private IReadOnlyCollection<Cards.Card> CardList => CardDict.Keys;


    //Process
    public override void _Process(double delta)
    {
        MovePickedCard();
    }


    //Card Operation
    private void AddCard(CardInterface ci)
    {
        CardDict.TryAdd(ci.Card, ci);
        AddChild(ci);
        ci.Selectable = false;

        InsertCard(ci);
        UpdateCardPosition();
    }

    private void AddCard(Cards.Card card)
    {
        var ci = CardInterface.GetInstance(card);
        ci.GlobalPosition = new(Project.ViewportSize.X / 2, Project.ViewportSize.Y + 32);
        AddCard(ci);
    }

    private void RemoveCard(Cards.Card card)
    {
        CardDict.Remove(card);
        UpdateCardPosition();
        SortCards();
    }

    private void RemoveCard(CardInterface ci) => RemoveCard(ci.Card);


    //Card Select
    private CardInterface? _selected;
    private CardInterface? Selected
    {
        get => _selected;
        set
        {
            if (Picked is not null) return;
            var old = _selected;
            var @new = value;
            _selected = value;

            OnSelectChanged(old, @new);
            if (old is not null) EventBus.Publish(new CardSelectedEvent(old.Card,   false));
            if (@new is not null) EventBus.Publish(new CardSelectedEvent(@new.Card, true));
        }
    }

    private int SelectedCardIndex => Selected is not null ? CardInterfaceList.ToList().IndexOf(Selected) : -1; //ToList回头凹一下


    //Card Drag & Use
    private CardInterface? _picked;
    private CardInterface? Picked
    {
        get => _picked;
        set
        {
            var oldOut = PickedCardOutField;
            var old = _picked;
            var @new = value;
            _picked = value;

            if (old is not null)
            {
                old.Selectable = false;
                if (!oldOut || !old.Card.Use(Play.State.Player)) //偷懒了，先检查oldOut再进行TryUse，任何一个失败则执行InsertCard
                    InsertCard(old);
                EventBus.Publish(new CardPickedEvent(old.Card, false));
            }

            if (@new is not null)
            {
                PickUpCard(@new);
                EventBus.Publish(new CardPickedEvent(@new.Card, true));
            }
        }
    }
    private Vector2 PickOffset { get; set; }

    private bool PickedCardOutField => Picked?.Position.Y < 0;

    private void MovePickedCard()
    {
        if (Picked is null) return;
        Picked.Position = GetLocalMousePosition() + PickOffset;
    }

    private void PickUpCard(CardInterface card)
    {
        PickOffset = -card.GetLocalMousePosition();
        card.XPosTween?.Kill();
        card.YPosTween?.Kill();
    }


    //Event handlers
    public override void _EnterTree()
    {
        EventHandlerSubscriber.InstanceSubscribe(this);
    }

    public override void _ExitTree()
    {
        EventHandlerSubscriber.InstanceUnsubscribe(this);
    }

    [EventHandler] [UsedImplicitly]
    private void OnCardInterfaceSelected(CardInterfaceSelectedEvent e)
    {
        if (!CardInterfaceList.Contains(e.CardInterface)) return;
        if (e.Selected)
            Selected = e.CardInterface;
        else if (Selected == e.CardInterface)
            Selected = null;
    }

    [EventHandler] [UsedImplicitly]
    private void OnCardInterfacePressed(CardInterfacePressedEvent e)
    {
        if (!CardInterfaceList.Contains(e.CardInterface)) return;
        if (e.Pressed)
            Picked = e.CardInterface;
        else if (Picked == e.CardInterface)
            Picked = null;
    }

    [EventHandler] [UsedImplicitly]
    private void OnCardPileInserted(CardPileInsertedEvent e)
    {
        if (e.CardPile != Pile) return;
        AddCard(e.Card);
    }

    [EventHandler] [UsedImplicitly]
    private void OnCardPileRemoved(CardPileRemovedEvent e)
    {
        if (e.CardPile != Pile) return;
        RemoveCard(e.Card);
    }
}