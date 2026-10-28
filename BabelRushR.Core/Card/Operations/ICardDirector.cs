namespace BabelRushR.Core.Card.Operations;

public interface ICardDirector
{
    void PlayCard(ICard card);
    bool TryPlayCard(ICard card);
}