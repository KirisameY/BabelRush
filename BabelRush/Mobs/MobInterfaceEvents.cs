using BabelRush.Gui.Mob;

using KirisameLib.Events;

namespace BabelRush.Mobs;

//Base
public record MobInterfaceEvent(MobInterface Interface) : BaseEvent;

//Select
public record MobInterfaceSelectedEvent(MobInterface Interface, bool Selected) : MobInterfaceEvent(Interface);