using BabelRush.Level.Stages;

namespace BabelRush.Gui.Screens.InGame;

public abstract record MapScreenEvent(MapScreen MapScreen) : GuiEvent;

public sealed record MapScreenChosenNodeEvent(MapScreen MapScreen, StageNode Node) : MapScreenEvent(MapScreen);