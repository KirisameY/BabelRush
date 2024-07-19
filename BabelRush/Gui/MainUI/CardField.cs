using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Cards;
using BabelRush.Gui.Card;

using Godot;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control, ICardContainer
{
    //Const
    private const float LeftPos = 0;
    private const float RightPos = 400;
    private const float MidPos = (LeftPos + RightPos) / 2;
    private const float CardRadius = 30;
    private const float CardInterval = 2;
    private const float CardYOffset = 32;
    private const float SelectedCardYOffset = 16;

    private const double InsertInterval = 0.15;
    private const double MoveInterval = 0.2;
    private const double SelectInterval = 0.1;
    private const double SortingInterval = 0.1;


    //Member
    private List<CardInterface> CardList { get; } = [];


    //Init
    public override void _Ready() { }


    //Card Operation
    public void AddCard(CardInterface card)
    {
        CardList.Add(card);
        AddChild(card);
        card.Selectable = false;
        card.Container = this;
        
        var tween = card.YPosTween = CreateTween();
        tween.TweenMethod(Callable.From((float y) => card.SetPositionY(y)), card.Position.Y, CardYOffset, InsertInterval)
             .SetTrans(Tween.TransitionType.Quart)
             .SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => card.Selectable = true));
        UpdateCardPosition();
    }

    public void AddCard(ICard card)
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
}