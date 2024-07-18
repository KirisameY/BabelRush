using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Cards;

using Godot;

namespace BabelRush.Gui.MainUI;

public partial class CardField : Control
{
    //Const
    private const float LeftPos = 0;
    private const float RightPos = 400;
    private const float MidPos = (LeftPos + RightPos) / 2;
    private const float CardRadius = 30;
    private const float CardInterval = 4;
    private const float CardYOffset = 24;

    private const double InsertInterval = 0.15;
    private const double MoveInterval = 0.2;


    //Member
    private List<CardInterface> CardList { get; } = [];
    private List<Tween> TweenList { get; set; } = [];
    private CardInterface SelectedCard { get; set; }
    private int SelectedCardIndex => CardList.IndexOf(SelectedCard);


    //Init
    public override void _Ready() { }


    //Card Operation
    public void AddCard(CardInterface card)
    {
        CardList.Add(card);
        AddChild(card);
        card.Selectable = false;
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

    private float[] CalculateCardXPosition()
    {
        var count = CardList.Count;

        var posRadius = (CardInterval + CardRadius) * (count - 1);
        var lPos = Math.Max(LeftPos + CardRadius, MidPos - posRadius);
        var rPos = Math.Min(RightPos - CardRadius, MidPos + posRadius);
        var deltaPos = count > 1 ? (rPos - lPos) / (count - 1) : 0;

        float[] result = new float[count];
        for (int i = 0; i < count; i++)
        {
            result[i] = lPos + i * deltaPos;
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