namespace BabelRush.Gui.Card;

public record CardInterfaceEvent(CardInterface CardInterface) : GuiEvent;

public record CardInterfaceClickedEvent(CardInterface CardInterface) : CardInterfaceEvent(CardInterface);

public record CardInterfaceSelectedEvent(CardInterface CardInterface, bool Selected) : CardInterfaceEvent(CardInterface);

public record CardInterfacePressedEvent(CardInterface CardInterface, bool Pressed) : CardInterfaceEvent(CardInterface);