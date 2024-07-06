using KirisameLib.Events;

namespace KirisameLib.I18n;

public record LocalChangedEvent(string PreviousLocal, string NewLocal) : BaseEvent;