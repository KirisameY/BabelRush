using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Gui.Utils;

using Godot;

using CardInterface = BabelRush.Gui.Cards.CardInterface;

namespace BabelRush.Gui.MainUI;

partial class CardField
{
    #region Consts

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

    #endregion


    #region Private Methods

    private float[] CalculateCardXPosition()
    {
        var count = CardInterfaces.Count;

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
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame); //wait 1 frame

        foreach ((CardInterface card, float xPos) in CardInterfaces.Zip(CalculateCardXPosition()))
        {
            card.XPosTween?.Kill();
            var tween = card.XPosTween = CreateTween();
            tween.TweenProperty(card, NodePaths.PositionX, xPos, MoveInterval)
                 .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }
    }

    private void InsertCard(CardInterface card)
    {
        card.YPosTween?.Kill();
        var tween = card.YPosTween = CreateTween();
        tween.TweenProperty(card, NodePaths.PositionY, CardYOffset, InsertInterval)
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
            var tween = old.YPosTween = CreateTween();
            tween.TweenProperty(old, NodePaths.PositionY, CardYOffset, SelectInterval)
                 .SetTrans(Tween.TransitionType.Quart)
                 .SetEase(Tween.EaseType.In);
            old.YPosTween.TweenCallback(Callable.From(SortCards));
        }

        if (@new is not null)
        {
            @new.YPosTween?.Kill();
            var tween = @new.YPosTween = CreateTween();
            tween.TweenProperty(@new, NodePaths.PositionY, SelectedCardYOffset, SelectInterval)
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

            int i = 0;
            Stack<CardInterface> backStack = [];
            foreach (var cardInterface in CardInterfaces)
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
            foreach (var card in CardInterfaces)
            {
                card.MoveToFront();
            }
        }
    }

    #endregion
}