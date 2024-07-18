using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Cards;

using Godot;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control, ICardContainer
{
    //Const
    private const float LeftPos = 0;
    private const float RightPos = 400;
    private const float MidPos = (LeftPos + RightPos) / 2;
    private const float CardRadius = 30;
    private const float CardInterval = 4;
    private const float CardYOffset = 32;
    private const float SelectedCardYOffset = 24;

    private const double InsertInterval = 0.15;
    private const double MoveInterval = 0.2;
    private const double SelectInterval = 0.1;


    //Member
    private List<CardInterface> CardList { get; } = [];
    private List<Tween> TweenList { get; set; } = [];


    //Init
    public override void _Ready() { }


    //Card Operation
    public void AddCard(CardInterface card)
    {
        CardList.Add(card);
        AddChild(card);
        card.Selectable = false;
        card.Container = this;

        var tween = CreateTween();
        tween.TweenMethod(Callable.From((float y) => card.SetPositionY(y)), card.Position.Y, CardYOffset, InsertInterval);
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
        if (Selected is null) Selected = card;
    }

    public void CardUnselected(CardInterface card)
    {
        if (Selected == card) Selected = null;
    }

    private void OnSelectChanged(CardInterface? old, CardInterface? @new)
    {
        if (old is not null)
        {
            CreateTween().TweenMethod(Callable.From((float y) => old.SetPositionY(y)),
                                      old.Position.Y, CardYOffset, SelectInterval);
        }

        if (@new is not null)
        {
            CreateTween().TweenMethod(Callable.From((float y) => @new.SetPositionY(y)),
                                      @new.Position.Y, SelectedCardYOffset, SelectInterval);
            var newIndex = CardList.IndexOf(@new);
            for (int i = CardList.Count - 1; i >= newIndex; i--)
            {
                CardList[i].MoveToFront();
            }
        }
        else
        {
            foreach (var card in CardList)
            {
                card.MoveToFront();
            }
        }

        UpdateCardPosition();
    }


    //UI dynamic
    private float[] CalculateCardXPosition()
    {
        var count = CardList.Count;

        var posRadius = (CardInterval + CardRadius) * (count - 1);
        var lPos = Math.Max(LeftPos + CardRadius, MidPos - posRadius);
        var rPos = Math.Min(RightPos - CardRadius, MidPos + posRadius);
        var deltaPos = count > 1 ? (rPos - lPos) / (count - 1) : 0;

        float[] result = new float[count];
        if (Selected is null)
        {
            for (int i = 0; i < count; i++)
            {
                result[i] = lPos + i * deltaPos;
            }
        }
        else
        {
            var sIndex = SelectedCardIndex;
            var sPos = result[sIndex] = lPos + sIndex * deltaPos;
            var lDelta = sIndex > 1 ? (sPos - lPos) / sIndex : 0;
            var rDelta = (count - sIndex - 1 > 0) ? (rPos - sPos) / (count - sIndex - 1) : 0;
            for (int i = 0; i < sIndex; i++)
            {
                result[i] = lPos + i * lDelta;
            }

            for (int i = 1; sIndex + i < count; i++)
            {
                result[sIndex + i] = sPos + i * rDelta;
            }
        }

        return result;
    }

    private void UpdateCardPosition()
    {
        foreach ((CardInterface card, float xPos) in CardList.Zip(CalculateCardXPosition()))
        {
            card.PosTween?.Kill();
            card.PosTween = CreateTween();
            card.PosTween.TweenMethod(Callable.From((float x) => card.SetPositionX(x)), card.Position.X, xPos, MoveInterval)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }
    }
}