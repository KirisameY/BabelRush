using KirisameY.Registration.Data;

namespace BabelRushR.Core.EntityAction.Types;

public class PlaceHolderEntityActionType : IEntityActionType
{
    private PlaceHolderEntityActionType() { }
    public static PlaceHolderEntityActionType Instance => field ??= new();


    public RegKey ID => field ??= RegKey.From("core", "404");

    public IEntityAction CreateAction(int value) => new DelegateEntityAction(_ => { });
}