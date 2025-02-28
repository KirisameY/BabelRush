using KirisameLib.Event;

namespace BabelRush.Cards;

//Base
public abstract record CardEvent(Card Card) : BaseEvent;

//Actions
public sealed record CardSelectedEvent(Card Card, bool Selected) : CardEvent(Card);

public sealed record CardPickedEvent(Card Card, bool Picked) : CardEvent(Card);

public sealed record CardUseRequestEvent(Card Card, CancelToken Cancel) : CardEvent(Card);

public sealed record CardUsedEvent(Card Card, bool CostAp,Variable<bool> ToExhaust) : CardEvent(Card);

//Gaming
public sealed record CardDrawnEvent(Card Card) : CardEvent(Card);

public sealed record CardDiscardRequestEvent(Card Card, CancelToken Cancel) : CardEvent(Card);

public sealed record CardDiscardedEvent(Card Card) : CardEvent(Card);

public sealed record CardExhaustRequestEvent(Card Card, CancelToken Cancel) : CardEvent(Card);

public sealed record CardExhaustedEvent(Card Card) : CardEvent(Card);

//Pile
public sealed record CardInsertedToPileEvent(CardPile CardPile, Card Card) : CardEvent(Card);

public sealed record CardRemovedFromPileEvent(CardPile CardPile, Card Card) : CardEvent(Card);

//Hub
public sealed record CardIntoHubEvent(Card Card) : CardEvent(Card);

public sealed record CardOutOfHubEvent(Card Card) : CardEvent(Card);