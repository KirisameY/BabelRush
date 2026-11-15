using KirisameY.Registration.Data;

namespace BabelRushR.Core.Level;

public sealed class PlaceHolderRoomDefinition : IRoomDefinition
{
    private PlaceHolderRoomDefinition() { }

    public static PlaceHolderRoomDefinition Instance => field ??= new();

    public RegKey ID => field ??= RegKey.From("core", "404");

    public double Length => 0;

    public IEnumerable<RoomContent> CreateContents(RoomContentContext context) => [];
}
