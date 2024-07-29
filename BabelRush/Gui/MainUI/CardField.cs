using System.Collections.Generic;

using BabelRush.Cards;
using BabelRush.GamePlay;
using BabelRush.Gui.Card;
using BabelRush.Gui.Mob;

using Godot;

using KirisameLib.Events;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control
{
    //Init
    public CardField()
    {
        EventBus.Register<MobInterfaceSelectedEvent>(OnMobInterfaceSelected);
    }

    public override void _Ready() { }


    //Member
    private List<CardInterface> CardList { get; } = [];


    //Process
    public override void _Process(double delta)
    {
        MovePickedCard();
    }


    //Card Operation
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
                EventBus.Publish(new CardPickedEvent(old.Card,   false));
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
        //Todo ,记得改成正确的机制
        if (FocusedMob is null) return false;
        card.Card.Use(Play.State.Player, [FocusedMob]);

        RemoveCard(card);

        return true;
    }


    //Event handlers
    public override void _EnterTree()
    {
        EventBus.Register<CardInterfaceSelectedEvent>(OnCardInterfaceSelectedEvent);
        EventBus.Register<CardInterfacePressedEvent>(OnCardInterfacePressedEvent);
    }

    public override void _ExitTree()
    {
        EventBus.Unregister<CardInterfaceSelectedEvent>(OnCardInterfaceSelectedEvent);
        EventBus.Unregister<CardInterfacePressedEvent>(OnCardInterfacePressedEvent);
    }

    public void OnCardInterfaceSelectedEvent(CardInterfaceSelectedEvent e)
    {
        if (!CardList.Contains(e.CardInterface)) return;
        if (e.Selected)
            Selected = e.CardInterface;
        else if (Selected == e.CardInterface)
            Selected = null;
    }

    public void OnCardInterfacePressedEvent(CardInterfacePressedEvent e)
    {
        if (!CardList.Contains(e.CardInterface)) return;
        if (e.Pressed)
            Picked = e.CardInterface;
        else if (Picked == e.CardInterface)
            Picked = null;
    }


    //Todo , move them out
    //新机制可以做成两段：卡牌举起时通知目标选择器，然后剩下的全权交给目标选择器处理，打出时直接通知选择器打出。
    private Mobs.Mob? FocusedMob { get; set; }

    private void OnMobInterfaceSelected(MobInterfaceSelectedEvent e)
    {
        if (e.Selected) FocusedMob = e.Interface.Mob;
        else if (e.Interface.Mob == FocusedMob) FocusedMob = null;
    }
}