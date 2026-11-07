using KirisameY.Registration.Data;

namespace BabelRushR.Core.Entity;

public class PlaceHolderEntity() : EntityBase(24)
{
    public override RegKey DisplayKey => RegKey.From("core", "404");
}