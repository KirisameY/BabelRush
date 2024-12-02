using KirisameLib.Event;

namespace BabelRush.Cards;

public abstract record CardHubEvent : BaseEvent;

public sealed record CardsShuffledEvent : CardHubEvent;