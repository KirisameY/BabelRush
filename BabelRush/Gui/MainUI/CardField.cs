using System.Collections.Generic;

using BabelRush.Cards;
using BabelRush.Gui.Card;

using Godot;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control, ICardContainer
{
    //Member
    private List<CardInterface> CardList { get; } = [];


    //Init
    public override void _Ready() { }


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

    private CardInterface? _picked;
    private CardInterface? Picked
    {
        get => _picked;
        set
        {
            var old = _picked;
            var @new = value;
            _picked = value;

            if (old is not null)
            {
                old.Selectable = false;
                InsertCard(old);
            }

            if (@new is not null) PickUpCard(@new);
        }
    }

    public void CardPressed(CardInterface card)
    {
        Picked = card;
    }

    public void CardReleased(CardInterface card)
    {
        if (Picked == card) Picked = null;
    }

    public void CardClicked(CardInterface card) { }


    //Card Drag
    private Vector2 PickOffset { get; set; }

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
}