using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.Registers;

namespace BabelRush.Registering.Registers;

public abstract class SubRegister
{
    private static class Cache<TItem>
    {
        public static readonly Dictionary<(IRegister<RegKey, TItem> parentRegister, string subPath), SubRegister<TItem>> Values = new();
    }

    public static SubRegister<TItem> Get<TItem>(IRegister<RegKey, TItem> parentRegister, string subPath)
    {
        var t = (parentRegister, subPath);
        if (!Cache<TItem>.Values.TryGetValue(t, out var reg))
            Cache<TItem>.Values[t] = reg = new SubRegister<TItem>(parentRegister, subPath);
        return reg;
    }
}

public class SubRegister<TItem>(IRegister<RegKey, TItem> parentRegister, string subPath) : SubRegister, IRegister<RegKey, TItem>
{
    private RegKey GetFullId(RegKey id) => subPath is "" ? id : RegKey.From(id.NameSpace, $"{subPath}/{id.Key})");

    public TItem GetItem(RegKey id) => parentRegister.GetItem(GetFullId(id));

    public bool ItemRegistered(RegKey id) => parentRegister.ItemRegistered(GetFullId(id));

    public TItem this[RegKey id] => parentRegister[GetFullId(id)];
}