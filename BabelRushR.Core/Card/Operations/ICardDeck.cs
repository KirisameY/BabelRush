using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

using BabelRushR.Core.Common;

using KirisameY.CollectionViews.Combined;
using KirisameY.NotifiableCollections.Collections;
using KirisameY.Relinq.Extensions;

namespace BabelRushR.Core.Card.Operations;

public interface ICardDeck
{
    [Immutable] IReadOnlyNotifiableList<ICard> HandPile { get; }
    [Immutable] IReadOnlyNotifiableList<ICard> DrawPile { get; }
    [Immutable] IReadOnlyNotifiableList<ICard> DiscardPile { get; }

    bool AddCard(ICard card, CardPile targetPile, int pos = -1);
    bool MoveCard(CardPile fromPile, int fromPos, CardPile targetPile, int targetPos = -1);
    bool SetCardTo(ICard card, CardPile targetPile, int pos = -1);
    bool RemoveCard(CardPile fromPile, int pos, [MaybeNullWhen(false)] out ICard removed);
    public bool RemoveCard(ICard card);
}

public static class CardDeckExtensions
{
    private static readonly ConditionalWeakTable<ICardDeck, ImmutableList<IReadOnlyNotifiableList<ICard>>> PilesCache = [];
    private static readonly ConditionalWeakTable<ICardDeck, IReadOnlyList<ICard>> FullDeckCache = [];

    extension(ICardDeck cardDeck)
    {
        public ImmutableList<IReadOnlyNotifiableList<ICard>> Piles =>
            PilesCache.GetOrAdd(cardDeck, deck => [deck.HandPile, deck.DrawPile, deck.DiscardPile]);

        public IReadOnlyList<ICard> FullDeck => FullDeckCache.GetOrAdd(cardDeck, deck => CombinedListView.Create(deck.Piles));

        public IReadOnlyList<ICard> CardsOf(CardPile pile) => pile switch
        {
            CardPile.None    => ImmutableList<ICard>.Empty,
            CardPile.Hand    => cardDeck.HandPile,
            CardPile.Draw    => cardDeck.DrawPile,
            CardPile.Discard => cardDeck.DiscardPile,
            _                => throw new ArgumentOutOfRangeException() // todo: 等待 closed enums，byd 这东西跳票到了 C# 16 得多等一年
        };

        public (CardPile Pile, int Index) FindCard(ICard card)
        {
            foreach (var pile in CardPile.GetValues().Where(pile => pile >= 0))
            {
                if (cardDeck.CardsOf(pile).IndexOf(card) is >= 0 and var i) return (pile, i);
            }
            return (CardPile.None, -1);
        }
    }
}