using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Gui.MainUI;

public partial class CardField
{
    //Const
    private const float LeftPos = 0;
    private const float RightPos = 400;
    private const float MidPos = (LeftPos + RightPos) / 2;
    private const float CardRadius = 30;
    private const float CardInterval = 1;
    private const float CardYOffset = 32;
    private const float SelectedCardYOffset = 16;

    private const double InsertInterval = 0.15;
    private const double MoveInterval = 0.2;
    private const double SelectInterval = 0.1;

    //Func
    private float[] CalculateCardXPosition()
    {
        var count = CardInterfaceList.Count;

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

    private async void UpdateCardPosition()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);//wait 1 frame
        
        foreach ((CardInterface card, float xPos) in CardInterfaceList.Zip(CalculateCardXPosition()))
        {
            card.XPosTween?.Kill();
            card.XPosTween = CreateTween();
            card.XPosTween.TweenMethod(Callable.From((float x) => card.SetPositionX(x)), card.Position.X, xPos, MoveInterval)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }
    }

    private void InsertCard(CardInterface card)
    {
        card.YPosTween?.Kill();
        var tween = card.YPosTween = CreateTween();
        tween.TweenMethod(Callable.From((float y) => card.SetPositionY(y)), card.Position.Y, CardYOffset, InsertInterval)
             .SetTrans(Tween.TransitionType.Quart)
             .SetEase(Tween.EaseType.Out);
        tween.TweenCallback(Callable.From(() => card.Selectable = true));
        tween.TweenCallback(Callable.From(SortCards));
        UpdateCardPosition();
    }

    private void OnSelectChanged(CardInterface? old, CardInterface? @new)
    {
        if (old is not null && old.Selectable)
        {
            old.YPosTween?.Kill();
            old.YPosTween = CreateTween();
            old.YPosTween
               .TweenMethod(Callable.From((float y) => old.SetPositionY(y)), old.Position.Y, CardYOffset, SelectInterval)
               .SetTrans(Tween.TransitionType.Quart)
               .SetEase(Tween.EaseType.In);
            old.YPosTween.TweenCallback(Callable.From(SortCards));
        }

        if (@new is not null)
        {
            @new.YPosTween?.Kill();
            @new.YPosTween = CreateTween();
            @new.YPosTween
                .TweenMethod(Callable.From((float y) => @new.SetPositionY(y)), @new.Position.Y, SelectedCardYOffset, SelectInterval)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            SortCards();
        }
    }

    private void SortCards()
    {
        if (Selected is not null)
        {
            var selectedIndex = SelectedCardIndex;

            // for (int i = 0; i < selectedIndex; i++)
            // {
            //     CardInterfaceList[i].MoveToFront();
            // }

            // for (int i = CardInterfaceList.Count - 1; i >= selectedIndex; i--)
            // {
            //     CardInterfaceList[i].MoveToFront();
            // }

            int i = 0;
            Stack<CardInterface> backStack = [];
            foreach (var cardInterface in CardInterfaceList)
            {
                if (i < selectedIndex)
                    cardInterface.MoveToFront();
                else
                    backStack.Push(cardInterface);
                i++;
            }
            foreach (var cardInterface in backStack)
            {
                cardInterface.MoveToFront();
            }
        }
        else
        {
            foreach (var card in CardInterfaceList)
            {
                card.MoveToFront();
            }
        }
    }
}