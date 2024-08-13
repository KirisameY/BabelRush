using KirisameLib.Events;

namespace BabelRush.Cards;

//CardEvents
public abstract record CardEvent(Card Card) : BaseEvent;

public sealed record CardSelectedEvent(Card Card, bool Selected) : CardEvent(Card);

public sealed record CardPickedEvent(Card Card, bool Picked) : CardEvent(Card);

public sealed record BeforeCardUseEvent(Card Card, CancelToken Cancel) : CardEvent(Card);

public sealed record CardUsedEvent(Card Card, Variable<bool> ToExhaust) : CardEvent(Card);

public sealed record CardDiscardEvent(Card Card) : CardEvent(Card);

public sealed record CardExhaustEvent(Card Card) : CardEvent(Card);

//CardPileEvents
public abstract record CardPileEvent(CardPile CardPile) : BaseEvent;

public sealed record CardPileInsertedEvent(CardPile CardPile, Card Card) : CardPileEvent(CardPile);

public sealed record CardPileRemovedEvent(CardPile CardPile, Card Card) : CardPileEvent(CardPile);