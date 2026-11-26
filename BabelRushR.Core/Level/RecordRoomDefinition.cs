using System.Collections.Immutable;

using BabelRushR.Core.Scenery;
using BabelRushR.Core.Scenery.Objects;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Level;

public record RecordRoomDefinition(RegKey ID, double Length, ImmutableList<RecordRoomDefinition.Content> Contents) : IRoomDefinition
{
    public IEnumerable<RoomContent> CreateContents()
    {
        return Contents.Select(static c => new RoomContent(c.Object.CreateObject(), c.Offset));
    }

    public readonly record struct Content(ISceneObjectExpression Object, double Offset);
}