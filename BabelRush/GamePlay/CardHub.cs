using System.Collections.Generic;
using System.Linq;

using BabelRush.Cards;

using JetBrains.Annotations;

using KirisameLib.Core.Collections;
using KirisameLib.Core.Events;

namespace BabelRush.GamePlay;

public class CardHub
{
    //Pile
    public CardPile CardField { get; } = [];
    public CardPile DrawPile { get; } = [];
    public CardPile DiscardPile { get; } = [];
    public IEnumerable<CardPile> Piles
    {
        get
        {
            yield return CardField;
            yield return DrawPile;
            yield return DiscardPile;
        }
    }


    //Public Properties
    private IReadOnlyCollection<Card>? _cardsView;
    public IReadOnlyCollection<Card> CardsView =>
        _cardsView ??= new CombinedCollectionView<Card>(CardField, DrawPile, DiscardPile);


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


    //Card in/out hub
    private static readonly Stack<Card> InternalMovingCardStack = [];
    private void PrepareInternalMove(Card card) => InternalMovingCardStack.Push(card);

    [EventHandler] [UsedImplicitly]
    private static void OnCardInsertedToPile(CardInsertedToPileEvent e)
    {
        var hub = Play.CardHub;
        if (!hub.Piles.Contains(e.CardPile)) return;

        InternalMovingCardStack.TryPeek(out var movingCard);
        if (movingCard is not null && movingCard == e.Card) InternalMovingCardStack.Pop();
        else EventBus.Publish(new CardOutOfHubEvent(e.Card));
    }

    [EventHandler] [UsedImplicitly]
    private static void OnCardRemovedFromPile(CardRemovedFromPileEvent e)
    {
        var hub = Play.CardHub;
        if (!hub.Piles.Contains(e.CardPile)) return;

        InternalMovingCardStack.TryPeek(out var movingCard);
        if (movingCard is not null && movingCard == e.Card) return;
        EventBus.Publish(new CardOutOfHubEvent(e.Card));
    }
}