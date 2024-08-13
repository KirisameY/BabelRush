using System;
using System.Collections;
using System.Collections.Generic;

using BabelRush.GamePlay;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Cards;

public sealed class CardPile : IReadOnlyList<Card>, IDisposable
{
    //Init&Dispose
    public CardPile()
    {
        EventHandlerSubscriber.InstanceSubscribe(this);
    }

    public void Dispose()
    {
        EventHandlerSubscriber.InstanceUnsubscribe(this);
    }


    //List
    private List<Card> Cards { get; } = [];

    [MustDisposeResource]
    public IEnumerator<Card> GetEnumerator()
    {
        return Cards.GetEnumerator();
    }

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return ((IEnumerable)Cards).GetEnumerator();
    }

    public int Count => Cards.Count;
    public Card this[int index] => Cards[index];


    public bool AddCard(Card card)
    {
        if (Cards.Contains(card)) return false;

        Cards.Add(card);
        EventBus.Publish(new CardPileInsertedEvent(this, card));
        return true;
    }

    public bool RemoveCard(Card card)
    {
        if (!Cards.Contains(card)) return false;

        if (!Cards.Remove(card)) return false;
        EventBus.Publish(new CardPileRemovedEvent(this, card));
        return true;
    }


    //EventHandlers
    [EventHandler] [UsedImplicitly]
    private void OnCardDiscard(CardDiscardEvent e)
    {
        if (this == Play.State.PlayerInfo.DiscardPile)
            AddCard(e.Card);
        else
            RemoveCard(e.Card);
    }

    [EventHandler] [UsedImplicitly]
    private void OnCardExhaust(CardExhaustEvent e)
    {
        RemoveCard(e.Card);
    }

    [EventHandler] [UsedImplicitly]
    private void OnCardUsed(CardUsedEvent e)
    {
        RemoveCard(e.Card);
    }
}