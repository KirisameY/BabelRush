using System.Collections.Immutable;

using BabelRush.Level.Rooms;

using Godot;

namespace BabelRush.Level.Stages;

public sealed record StageNode(RoomTemplate Room, ImmutableArray<StageNode> NextRooms, int Ordinal, Vector2 DisplayPosition)
{
    public static StageNode Default { get; } = new StageNode(RoomTemplate.Default, [], 0, Vector2.Zero);
}