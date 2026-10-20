using BabelRushR.Core.Card;

namespace BabelRushR.Core.GamePlay.CardRelated;

public interface ICardDirector
{
    void PlayCard(ICard card);
    bool TryPlayCard(ICard card);
}