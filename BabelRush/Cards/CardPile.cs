using System.Collections;
using System.Collections.Generic;
using System.Linq;

using JetBrains.Annotations;

using KirisameLib.Events;

namespace BabelRush.Cards;

public sealed class CardPile : IReadOnlyCollection<Card>
{
    //List
    private LinkedList<Card> Cards { get; } = [];

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


    public bool AddCard(Card card, bool toTop = true)
    {
        if (Cards.Contains(card)) return false;

        if (toTop) Cards.AddFirst(card);
        else Cards.AddLast(card);

        EventBus.Publish(new CardPileInsertedEvent(this, card));
        return true;
    }

    public void AddCards(IEnumerable<Card> cards, bool toTop = true)
    {
        foreach (var card in cards)
        {
            AddCard(card, toTop);
        }
    }

    public bool RemoveCard(Card card)
    {
        if (!Cards.Contains(card)) return false;

        if (!Cards.Remove(card)) return false;
        EventBus.Publish(new CardPileRemovedEvent(this, card));
        return true;
    }

    private LinkedListNode<Card>? GetNode(int index, bool fromTop)
    {
        LinkedListNode<Card>? result;
        if (fromTop)
        {
            result = Cards.First;
            for (int i = 0; i < index && result is not null; i++)
                result = result.Next;
        }
        else
        {
            result = Cards.Last;
            for (int i = 0; i < index && result is not null; i++)
                result = result.Previous;
        }
        return result;
    }

    private IEnumerable<LinkedListNode<Card>> GetNodes(int count, bool fromTop)
    {
        if (fromTop)
        {
            var node = Cards.First;
            for (int i = 0; i < count && node is not null; i++)
            {
                yield return node;
                node = node.Next;
            }
        }
        else
        {
            var node = Cards.Last;
            for (int i = 0; i < count && node is not null; i++)
            {
                yield return node;
                node = node.Previous;
            }
        }
    }

    [Pure] public Card? GetCard(int index = 0, bool fromTop = true) => GetNode(index, fromTop)?.Value;

    [Pure] public List<Card> GetCards(int count = 1, bool fromTop = true) => GetNodes(count, fromTop).Select(x => x.Value).ToList();

    public Card? PickCard(int index = 0, bool fromTop = true)
    {
        var node = GetNode(index, fromTop);
        var result = node?.Value;
        if (node is not null) Cards.Remove(node);
        return result;
    }

    public List<Card> PickCards(int count = 1, bool fromTop = true)
    {
        var nodes = GetNodes(count, fromTop);
        var result = nodes.Select(node =>
        {
            Cards.Remove(node);
            return node.Value;
        }).ToList();
        return result;
    }

    public List<Card> TakeAll()
    {
        var node = Cards.First;
        List<Card> cards = [];
        while (node is not null)
        {
            Cards.Remove(node);
            cards.Add(node.Value);
            node = node.Next;
        }
        return cards;
    }
}