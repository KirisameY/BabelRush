namespace BabelRush.Gui.Mob;

//Base
public record MobInterfaceEvent(MobInterface Interface) : GuiEvent;

//Select
public record MobInterfaceSelectedEvent(MobInterface Interface, bool Selected) : MobInterfaceEvent(Interface);