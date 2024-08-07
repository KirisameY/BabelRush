using KirisameLib.Events;

namespace BabelRush.Cards;

public abstract record CardEvent(Card Card) : BaseEvent;

public sealed record CardSelectedEvent(Card Card, bool Selected) : CardEvent(Card);

public sealed record CardPickedEvent(Card Card, bool Picked) : CardEvent(Card);

public sealed record BeforeCardUseEvent(Card Card) : CardEvent(Card);

public sealed record CardUsedEvent(Card Card) : CardEvent(Card);