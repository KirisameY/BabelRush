using BabelRushR.Core.Level;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Test;

public class RoomDefinitionTests
{
    [Fact]
    public void Placeholder_Definition_Is_Shared_And_Empty()
    {
        var definition = PlaceHolderRoomDefinition.Instance;

        Assert.Same(definition, PlaceHolderRoomDefinition.Instance);
        Assert.Equal(RegKey.From("core", "404"), definition.ID);
        Assert.Equal(0, definition.Length);
        Assert.Empty(definition.CreateContents(default));
    }

    [Fact]
    public void Registry_Fallback_Is_The_Placeholder_Definition()
    {
        Assert.Same(PlaceHolderRoomDefinition.Instance, RoomDefinition.Fallback);
    }
}
