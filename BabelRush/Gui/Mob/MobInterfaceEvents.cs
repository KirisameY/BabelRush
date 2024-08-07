namespace BabelRush.Gui.Mob;

//Base
public abstract record MobInterfaceEvent(MobInterface Interface) : GuiEvent;

//Select
public sealed record MobInterfaceSelectedEvent(MobInterface Interface, bool Selected) : MobInterfaceEvent(Interface);