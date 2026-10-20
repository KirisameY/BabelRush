using BabelRushR.Core.Card;

using KirisameY.CollectionViews.Combined;
using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.GamePlay.CardRelated;

public class CommonCardDeck : ICardDeck
{
    public NotifiableList<ICard> HandPile { get; } = new NotifiableList<ICard>();
    public NotifiableList<ICard> DrawPile { get; } = new NotifiableList<ICard>();
    public NotifiableList<ICard> DiscardPile { get; } = new NotifiableList<ICard>();
}