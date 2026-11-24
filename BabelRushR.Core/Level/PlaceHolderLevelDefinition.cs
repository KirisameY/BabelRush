using BabelRushR.Core.GamePlay;

using KirisameY.Registration.Data;

namespace BabelRushR.Core.Level;

public class PlaceHolderLevelDefinition : ILevelDefinition
{
    private PlaceHolderLevelDefinition() { }
    public static PlaceHolderLevelDefinition Instance => field ??= new();

    public RegKey ID => field ??= RegKey.From("core", "404");

    public ILevel GenerateLevel(IGamePlay gamePlay) => new CommonLevel(
        new LevelMap(
            new LevelNode(PlaceHolderRoomDefinition.Instance, new MapPosition(0.5, 0.5), [])
        )
    );
}