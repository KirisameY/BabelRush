using System.Collections.Immutable;
using System.Runtime.CompilerServices;

using BabelRushR.Core.Card;
using BabelRushR.Core.Common;

using KirisameY.CollectionViews.Combined;
using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.GamePlay.CardRelated;

public interface ICardDeck
{
    [Immutable] INotifiableList<ICard> HandPile { get; }
    [Immutable] INotifiableList<ICard> DrawPile { get; }
    [Immutable] INotifiableList<ICard> DiscardPile { get; }
}

public static class CardDeckExtensions
{
    private static readonly ConditionalWeakTable<ICardDeck, ImmutableList<INotifiableList<ICard>>> PilesCache = [];
    private static readonly ConditionalWeakTable<ICardDeck, IReadOnlyList<ICard>> FullDeckCache = [];

    extension(ICardDeck cardDeck)
    {
        public ImmutableList<INotifiableList<ICard>> Piles =>
            PilesCache.GetOrAdd(cardDeck, deck => [deck.HandPile, deck.DrawPile, deck.DiscardPile]);

        public IReadOnlyList<ICard> FullDeck => FullDeckCache.GetOrAdd(cardDeck, deck => CombinedListView.Create(deck.Piles));

        public (INotifiableList<ICard> Pile, int Index)? FindCard(ICard card)
        {
            foreach (var pile in cardDeck.Piles)
            {
                if (pile.IndexOf(card) is >= 0 and var i) return (pile, i);
            }
            return null;
        }
    }
}