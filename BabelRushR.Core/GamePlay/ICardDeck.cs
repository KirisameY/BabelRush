using BabelRushR.Core.Card;

using KirisameY.NotifiableCollections.Collections;

namespace BabelRushR.Core.GamePlay;

public interface ICardDeck
{
    INotifiableList<ICard> HandPile { get; }
    INotifiableList<ICard> DrawPile { get; }
    INotifiableList<ICard> DiscardPile { get; }
}