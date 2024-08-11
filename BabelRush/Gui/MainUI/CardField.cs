using System.Collections.Generic;

using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Gui.Card;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Events;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control
{
    //Init
    public override void _Ready() { }


    //Member
    private List<CardInterface> CardList { get; } = [];


    //Process
    public override void _Process(double delta)
    {
        MovePickedCard();
    }


    //Card Operation
    //Todo: to private, use eventbus to call
    public void AddCard(CardInterface card)
    {
        CardList.Add(card);
        AddChild(card);
        card.Selectable = false;

        InsertCard(card);
        UpdateCardPosition();
    }

    public void AddCard(Cards.Card card)
    {
        var ci = CardInterface.GetInstance(card);
        ci.GlobalPosition = new(Project.ViewportSize.X / 2, Project.ViewportSize.Y + 32);
        AddCard(ci);
    }

    public void RemoveCard(CardInterface card)
    {
        CardList.Remove(card);
        card.QueueFree();
        UpdateCardPosition();
        SortCards();
    }


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

    private int SelectedCardIndex => Selected is not null ? CardList.IndexOf(Selected) : -1;


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
                if (!oldOut || !TryUseCard(old)) //偷懒了，先检查oldOut再进行TryUse，任何一个失败则执行InsertCard
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

    private bool TryUseCard(CardInterface card)
    {
        if (!card.Card.Use(Play.State.Player)) return false; // 若使用失败，返回false

        RemoveCard(card);

        return true;
    }


    //Event handlers
    public override void _EnterTree()
    {
        // EventBus.Register<CardInterfaceSelectedEvent>(OnCardInterfaceSelectedEvent);
        // EventBus.Register<CardInterfacePressedEvent>(OnCardInterfacePressedEvent);
        EventHandlerSubscriber.InstanceSubscribe(this);
    }

    public override void _ExitTree()
    {
        // EventBus.Unregister<CardInterfaceSelectedEvent>(OnCardInterfaceSelectedEvent);
        // EventBus.Unregister<CardInterfacePressedEvent>(OnCardInterfacePressedEvent);
        EventHandlerSubscriber.InstanceUnsubscribe(this);
    }

    [EventHandler] [UsedImplicitly]
    public void OnCardInterfaceSelectedEvent(CardInterfaceSelectedEvent e)
    {
        if (!CardList.Contains(e.CardInterface)) return;
        if (e.Selected)
            Selected = e.CardInterface;
        else if (Selected == e.CardInterface)
            Selected = null;
    }

    [EventHandler] [UsedImplicitly]
    public void OnCardInterfacePressedEvent(CardInterfacePressedEvent e)
    {
        if (!CardList.Contains(e.CardInterface)) return;
        if (e.Pressed)
            Picked = e.CardInterface;
        else if (Picked == e.CardInterface)
            Picked = null;
    }
}