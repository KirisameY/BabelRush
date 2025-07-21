using System.Collections.Generic;
using System.Collections.Immutable;

using BabelRush.Data;
using BabelRush.Data.ExtendModels;
using BabelRush.Registers;

using JetBrains.Annotations;

namespace BabelRush.Level.Stages;

[Model] [UsedImplicitly]
public partial class StageNodeModel
{
    [NecessaryProperty]
    public partial string RoomId { get; set; }

    [NecessaryProperty]
    public partial int Ordinal { get; set; }

    [NecessaryProperty]
    public partial Vector2Model DisplayPosition { get; set; }

    public StageNode Convert(string nameSpace, IEnumerable<StageNode> nextRooms)
    {
        RegKey roomId = RoomId.WithDefaultNameSpace(nameSpace);
        return new(LevelRegisters.Rooms[roomId], nextRooms.ToImmutableArray(), Ordinal, DisplayPosition);
    }
}