using BabelRushR.Core.GamePlay;

namespace BabelRushR.Core.Card.Operations;

public abstract record CardDeckEvent : GamePlayEvent;

public record CardDeckAddedCardEvent(ICard Card, CardPile Pile, int Position) : CardDeckEvent;

public record CardDeckMovedCardEvent(ICard Card, CardPile FromPile, int FromPosition, CardPile ToPile, int ToPosition) : CardDeckEvent;

public record CardDeckRemovedCardEvent(ICard Card, CardPile Pile, int Position) : CardDeckEvent;

// public record CardDeckPileShuffledEvent() : CardDeckEvent;