namespace BabelRush.Gui.Card;

public abstract record CardInterfaceEvent(CardInterface CardInterface) : GuiEvent;

public sealed record CardInterfaceClickedEvent(CardInterface CardInterface) : CardInterfaceEvent(CardInterface);

public sealed record CardInterfaceSelectedEvent(CardInterface CardInterface, bool Selected) : CardInterfaceEvent(CardInterface);

public sealed record CardInterfacePressedEvent(CardInterface CardInterface, bool Pressed) : CardInterfaceEvent(CardInterface);