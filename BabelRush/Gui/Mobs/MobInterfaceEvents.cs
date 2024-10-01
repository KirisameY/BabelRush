namespace BabelRush.Gui.Mobs;

//Base
public abstract record MobInterfaceEvent(MobInterface Interface) : GuiEvent;

//Select
public sealed record MobInterfaceSelectedEvent(MobInterface Interface, bool Selected) : MobInterfaceEvent(Interface);