using KirisameLib.Event;

namespace BabelRush.I18n;

public record LocalChangedEvent(string Prev, string Current) : BaseEvent;