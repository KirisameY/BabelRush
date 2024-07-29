using System;
using System.Collections.Frozen;
using System.Collections.Generic;

using BabelRush.GamePlay;
using BabelRush.Gui.Card;
using BabelRush.Mobs;

using Godot;

using KirisameLib.Events;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control, ICardContainer
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
        card.Container = this;

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
        }
    }

    private int SelectedCardIndex => Selected is not null ? CardList.IndexOf(Selected) : -1;

    public void CardSelected(CardInterface card)
    {
        Selected = card;
    }

    public void CardUnselected(CardInterface card)
    {
        if (Selected == card) Selected = null;
    }

    public void CardClicked(CardInterface card) { }


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
            }

            if (@new is not null) PickUpCard(@new);
        }
    }
    private Vector2 PickOffset { get; set; }

    public void CardPressed(CardInterface card)
    {
        Picked = card;
    }

    public void CardReleased(CardInterface card)
    {
        if (Picked == card) Picked = null;
    }

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


    //Todo , move them out
    //新机制可以做成两段：卡牌举起时通知目标选择器，然后剩下的全权交给目标选择器处理，打出时直接通知选择器打出。
    private Mobs.Mob? FocusedMob { get; set; }

    private void OnMobInterfaceSelected(MobInterfaceSelectedEvent e)
    {
        if (e.Selected) FocusedMob = e.Interface.Mob;
        else if (e.Interface.Mob == FocusedMob) FocusedMob = null;
    }
}