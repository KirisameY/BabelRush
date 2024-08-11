using System.Collections;
using System.Collections.Generic;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Cards;

public class CardPile : IReadOnlyList<Card>
{
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
}