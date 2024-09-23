using System.Collections.Generic;
using System.Linq;

using BabelRush.Cards;

using JetBrains.Annotations;

using KirisameLib.Core.Collections;
using KirisameLib.Core.Events;
using KirisameLib.Core.Extensions;
using KirisameLib.Core.RandomAsteroid;

namespace BabelRush.GamePlay;

public class CardHub(RandomBelt Random)
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

        PrepareInternalMove(card);
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

    /// <summary>
    /// Draw a card to card field from draw pile.
    /// </summary>
    /// <returns>True if there is any card in draw pile, and drawn to card field. Otherwise, false.</returns>
    public bool DrawCard()
    {
        if (DrawPile.Count <= 0) return false;

        var card = DrawPile.GetCard()!;
        PrepareInternalMove(card);

        DrawPile.PickCard();
        CardField.AddCard(card);
        EventBus.Publish(new CardDrawnEvent(card));

        StopInternalMove();
        return true;
    }

    /// <summary>
    /// Draw cards to card field from draw pile.
    /// </summary>
    /// <param name="count"></param>
    /// <returns>-1 if all succeed, else the count of drawn card</returns>
    public int DrawCard(int count) => Enumerable.Range(0, count).FirstOrDefault(_ => !DrawCard(), -1);

    public bool ShuffleDiscarded()
    {
        if (DiscardPile.Count <= 0) return false;

        PrepareInternalMove(DiscardPile);
        var shuffled = Random.Shuffle(DiscardPile.TakeAll());
        DrawPile.AddCards(shuffled);
        EventBus.Publish(new CardsShuffledEvent());
        return true;
    }

    public bool DrawWithShuffleWhenNeed()
    {
        if (DrawPile.Count <= 0) ShuffleDiscarded();
        return DrawCard();
    }

    public int DrawWithShuffleWhenNeed(int count) => Enumerable.Range(0, count).FirstOrDefault(_ => !DrawWithShuffleWhenNeed(), -1);


    //Card in/out hub
    private readonly List<Card> _internalMovingCards = [];
    private CardPile? _internalMovingOutPile = null;
    private void PrepareInternalMove(Card card) => _internalMovingCards.Add(card);
    private void PrepareInternalMove(CardPile cardPile) => _internalMovingOutPile = cardPile;
    private void StopInternalMove() => _internalMovingOutPile = null;

    [EventHandler] [UsedImplicitly]
    private static void OnCardInsertedToPile(CardInsertedToPileEvent e)
    {
        var hub = Play.CardHub;
        if (!hub.Piles.Contains(e.CardPile)) return;

        if (!hub._internalMovingCards.Remove(e.Card)) //if not in moving cards
            EventBus.Publish(new CardIntoHubEvent(e.Card));
    }

    [EventHandler] [UsedImplicitly]
    private static void OnCardRemovedFromPile(CardRemovedFromPileEvent e)
    {
        var hub = Play.CardHub;
        if (!hub.Piles.Contains(e.CardPile)) return;

        if (hub._internalMovingCards.Contains(e.Card)) return;
        if (hub._internalMovingOutPile == e.CardPile) hub._internalMovingCards.Add(e.Card);
        else EventBus.Publish(new CardOutOfHubEvent(e.Card));
    }
}