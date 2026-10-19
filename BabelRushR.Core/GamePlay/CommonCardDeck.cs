using BabelRushR.Core.Card;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.GamePlay;

public class CommonCardDeck : ICardDeck
{
    public INotifiableList<ICard> HandPile { get; } = new NotifiableList<ICard>();
    public INotifiableList<ICard> DrawPile { get; } = new NotifiableList<ICard>();
    public INotifiableList<ICard> DiscardPile { get; } = new NotifiableList<ICard>();
}