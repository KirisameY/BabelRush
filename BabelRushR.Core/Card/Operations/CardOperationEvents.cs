using KirisameY.EventBus.EventComponents;

namespace BabelRushR.Core.Card.Operations;

public abstract record CardOperationEvent(ICard Card) : CardEvent(Card);

public record CardPlayRequestEvent(ICard Card, EventCancelToken CancelToken) : CardOperationEvent(Card), ICancelableEvent;

public record CardCostCalculatingEvent(ICard Card) : CardOperationEvent(Card);

public record CardPlayingEvent(ICard Card) : CardOperationEvent(Card);

public record CardPlayedEvent(ICard Card) : CardOperationEvent(Card);