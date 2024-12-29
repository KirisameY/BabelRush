using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.GamePlay;

using KirisameLib.Collections;
using KirisameLib.Event;
using KirisameLib.Randomization;

namespace BabelRush.Cards;

[EventHandlerContainer]
public partial class CardHub(RandomBelt random)
{
    #region Piles

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

    #endregion


    #region Public Properties

    [field: AllowNull, MaybeNull]
    public IReadOnlyCollection<Card> CardsView =>
        field ??= new CombinedCollectionView<Card>(CardField, DrawPile, DiscardPile);

    #endregion


    #region Public Methods

    /// <returns>False if cancelled</returns>
    public async ValueTask<bool> DiscardCard(Card card, bool cancellable = true)
    {
        var canceled = (
            await Game.EventBus.PublishAndWaitFor(new BeforeCardDiscardEvent(card, new()))
        ).Cancel.Canceled;
        if (cancellable && canceled) return false;

        PrepareInternalMove(card);
        CardField.RemoveCard(card);
        DrawPile.RemoveCard(card);
        DiscardPile.AddCard(card);

        Game.EventBus.Publish(new CardDiscardedEvent(card));
        return true;
    }

    /// <returns>False if cancelled</returns>
    public async ValueTask<bool> ExhaustCard(Card card, bool cancellable = true)
    {
        var canceled = (
            await Game.EventBus.PublishAndWaitFor(new BeforeCardExhaustEvent(card, new()))
        ).Cancel.Canceled;
        if (cancellable && canceled) return false;

        CardField.RemoveCard(card);
        DrawPile.RemoveCard(card);
        DiscardPile.RemoveCard(card);

        Game.EventBus.Publish(new CardExhaustedEvent(card));
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
        Game.EventBus.Publish(new CardDrawnEvent(card));

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
        var shuffled = random.Shuffle(DiscardPile.TakeAll());
        DrawPile.AddCards(shuffled);
        Game.EventBus.Publish(new CardsShuffledEvent());
        return true;
    }

    public bool DrawWithShuffleWhenNeed()
    {
        if (DrawPile.Count <= 0) ShuffleDiscarded();
        return DrawCard();
    }

    public int DrawWithShuffleWhenNeed(int count) => Enumerable.Range(0, count).FirstOrDefault(_ => !DrawWithShuffleWhenNeed(), -1);

    #endregion


    #region Auto operation

    private void DetectDrawWhenPileInsert(CardInsertedToPileEvent e)
    {
        if (e.CardPile == DrawPile || e.CardPile == DiscardPile) DrawToMinValue();
    }

    private void DetectDrawWhenPileRemove(CardRemovedFromPileEvent e)
    {
        if (e.CardPile == CardField) DrawToMinValue();
    }

    private void DrawToMinValue()
    {
        while (CardField.Count < Play.MinCardValue)
        {
            if (!DrawWithShuffleWhenNeed()) break;
        }
    }

    #endregion


    #region Card in/out hub

    private readonly List<Card> _internalMovingCards = [];
    private CardPile? _internalMovingOutPile = null;
    private void PrepareInternalMove(Card card) => _internalMovingCards.Add(card);
    private void PrepareInternalMove(CardPile cardPile) => _internalMovingOutPile = cardPile;
    private void StopInternalMove() => _internalMovingOutPile = null;

    private void CardInDecide(CardInsertedToPileEvent e)
    {
        if (_internalMovingCards.Remove(e.Card)) //if not in moving cards
            Game.EventBus.Publish(new CardIntoHubEvent(e.Card));
    }

    private void CardOutDecide(CardRemovedFromPileEvent e)
    {
        if (_internalMovingCards.Contains(e.Card)) return;
        if (_internalMovingOutPile == e.CardPile) _internalMovingCards.Add(e.Card);
        else Game.EventBus.Publish(new CardOutOfHubEvent(e.Card));
    }

    #endregion


    #region EventHandlers

    [EventHandler]
    private static void OnCardInsertedToPile(CardInsertedToPileEvent e)
    {
        var hub = Play.CardHub;
        if (!hub.Piles.Contains(e.CardPile)) return;

        hub.CardInDecide(e);
        hub.DetectDrawWhenPileInsert(e);
    }


    [EventHandler]
    private static void OnCardRemovedFromPile(CardRemovedFromPileEvent e)
    {
        var hub = Play.CardHub;
        if (!hub.Piles.Contains(e.CardPile)) return;

        hub.CardOutDecide(e);
        hub.DetectDrawWhenPileRemove(e);
    }

    #endregion
}