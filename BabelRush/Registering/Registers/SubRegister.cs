using BabelRush.Data;

using KirisameLib.Data.Registers;

namespace BabelRush.Registering.Registers;

public abstract class SubRegister
{
    public static SubRegister<TItem> Create<TItem>(IRegister<RegKey, TItem> parentRegister, string subPath) => new(parentRegister, subPath);
}

public class SubRegister<TItem>(IRegister<RegKey, TItem> parentRegister, string subPath) : SubRegister, IRegister<RegKey, TItem>
{
    private RegKey GetFullId(RegKey id) => subPath is "" ? id : RegKey.From(id.NameSpace, $"{subPath}/{id.Key})");

    public TItem GetItem(RegKey id) => parentRegister.GetItem(GetFullId(id));

    public bool ItemRegistered(RegKey id) => parentRegister.ItemRegistered(GetFullId(id));

    public TItem this[RegKey id] => parentRegister[GetFullId(id)];
}