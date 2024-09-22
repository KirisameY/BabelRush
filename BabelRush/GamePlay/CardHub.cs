using BabelRush.Cards;

using JetBrains.Annotations;

using KirisameLib.Core.Events;

namespace BabelRush.GamePlay;

public class CardHub
{
    //Pile
    public CardPile CardField { get; } = [];
    public CardPile DrawPile { get; } = [];
    public CardPile DiscardPile { get; } = [];


    //Todo:整体列表 ，进出整体卡组记录
    //Public Methods
    /// <returns>False if cancelled</returns>
    public bool DiscardCard(Card card, bool cancellable = true)
    {
        CancelToken cancelToken = new CancelToken();
        EventBus.Publish(new BeforeCardDiscardEvent(card, cancelToken));
        if (cancellable && cancelToken.Canceled) return false;

        CardField.RemoveCard(card);
        DrawPile.RemoveCard(card);
        DiscardPile.AddCard(card);

        EventBus.Publish(new CardDiscardedEvent(card));
        return true;
    }

    /// <returns>False if cancelled</returns>
    public bool ExhaustCard(Card card, bool cancellable = true)
    {
        CancelToken cancelToken = new CancelToken();
        EventBus.Publish(new BeforeCardExhaustEvent(card, cancelToken));
        if (cancellable && cancelToken.Canceled) return false;

        CardField.RemoveCard(card);
        DrawPile.RemoveCard(card);
        DiscardPile.RemoveCard(card);

        EventBus.Publish(new CardExhaustedEvent(card));
        return true;
    }
}