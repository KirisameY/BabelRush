using System;
using System.Linq;

using BabelRush.Cards;

using Godot;

using CardInterface = BabelRush.Gui.Card.CardInterface;

namespace BabelRush.Gui.MainUI;

public partial class CardField
{
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
            card.XPosTween?.Kill();
            card.XPosTween = CreateTween();
            card.XPosTween.TweenMethod(Callable.From((float x) => card.SetPositionX(x)), card.Position.X, xPos, MoveInterval)
                .SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        }
    }

    private void OnSelectChanged(CardInterface? old, CardInterface? @new)
    {
        if (old is not null)
        {
            old.YPosTween?.Kill();
            old.YPosTween = CreateTween();
            old.YPosTween
               .TweenMethod(Callable.From((float y) => old.SetPositionY(y)), old.Position.Y, CardYOffset, SelectInterval)
               .SetTrans(Tween.TransitionType.Quart)
               .SetEase(Tween.EaseType.In);
        }

        if (@new is not null)
        {
            @new.YPosTween?.Kill();
            @new.YPosTween = CreateTween();
            @new.YPosTween
                .TweenMethod(Callable.From((float y) => @new.SetPositionY(y)), @new.Position.Y, SelectedCardYOffset, SelectInterval)
                .SetTrans(Tween.TransitionType.Back)
                .SetEase(Tween.EaseType.Out);
            SortTween?.Kill();
            var newIndex = CardList.IndexOf(@new);
            for (int i = 0; i < newIndex; i++)
            {
                CardList[i].MoveToFront();
            }

            for (int i = CardList.Count - 1; i >= newIndex; i--)
            {
                CardList[i].MoveToFront();
            }
        }
        else
        {
            SortTween?.Kill();
            SortTween = CreateTween();
            SortTween.TweenCallback(Callable.From(() =>
            {
                foreach (var card in CardList)
                {
                    card.MoveToFront();
                }
            })).SetDelay(SortingInterval);
        }

        UpdateCardPosition();
    }

    private Tween? SortTween { get; set; }
}