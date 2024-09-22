using KirisameLib.Core.Events;

namespace BabelRush.Cards;

//Base
public abstract record CardEvent(Card Card) : BaseEvent;

//Actions
public sealed record CardSelectedEvent(Card Card, bool Selected) : CardEvent(Card);

public sealed record CardPickedEvent(Card Card, bool Picked) : CardEvent(Card);

public sealed record BeforeCardUseEvent(Card Card, CancelToken Cancel) : CardEvent(Card);

public sealed record CardUsedEvent(Card Card, Variable<bool> ToExhaust) : CardEvent(Card);

//Pile
public sealed record BeforeCardDiscardEvent(Card Card, CancelToken Cancel) : CardEvent(Card);

public sealed record CardDiscardedEvent(Card Card) : CardEvent(Card);

public sealed record BeforeCardExhaustEvent(Card Card, CancelToken Cancel) : CardEvent(Card);

public sealed record CardExhaustedEvent(Card Card) : CardEvent(Card);

public sealed record CardPileInsertedEvent(CardPile CardPile, Card Card) : CardEvent(Card);

public sealed record CardPileRemovedEvent(CardPile CardPile, Card Card) : CardEvent(Card);