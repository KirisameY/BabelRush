using System.Collections.Immutable;

using BabelRush.Scenery.Rooms;

using Godot;

namespace BabelRush.Scenery.Stages;

public sealed record StageNode(RoomTemplate Room, ImmutableArray<StageNode> NextRooms, int Ordinal, Vector2 DisplayPosition);