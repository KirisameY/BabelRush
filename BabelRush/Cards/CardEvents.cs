using KirisameLib.Events;

namespace BabelRush.Cards;

public record CardEvent(Card Card) : BaseEvent;

public record CardSelectedEvent(Card Card, bool Selected) : CardEvent(Card);

public record CardPickedEvent(Card Card, bool Picked) : CardEvent(Card);

public record BeforeCardUseEvent(Card Card) : CardEvent(Card);

public record CardUsedEvent(Card Card) : CardEvent(Card);