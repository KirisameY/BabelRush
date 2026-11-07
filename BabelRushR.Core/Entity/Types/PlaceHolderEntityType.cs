using KirisameY.Registration.Data;

namespace BabelRushR.Core.Entity.Types;

public class PlaceHolderEntityType : IEntityType
{
    private PlaceHolderEntityType() { }
    public static IEntityType Instance => field ??= new PlaceHolderEntityType();

    public RegKey ID => field ??= RegKey.From("core", "404");

    public IEntity CreateEntity() => new PlaceHolderEntity();
}