using BabelRush.Level.Scenery;

using KirisameLib.Event;

namespace BabelRush.Level.Rooms;

public abstract record RoomEvent(Room Room) : BaseEvent;

//
public sealed record ObjectEnteredRoomEvent(Room Room, SceneObject Object) : RoomEvent(Room);

public sealed record ObjectExitedRoomEvent(Room Room, SceneObject Object) : RoomEvent(Room);