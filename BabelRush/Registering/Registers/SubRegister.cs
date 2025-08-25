using System.Collections.Generic;

using BabelRush.Data;

using KirisameLib.Data.Registers;

namespace BabelRush.Registering.Registers;

public class SubRegister(IRegister<RegKey> parentRegister, string subPath) : IRegister<RegKey>
{
    protected readonly string SubPath = subPath;

    protected RegKey GetFullId(RegKey id) => SubPath is "" ? id : RegKey.From(id.NameSpace, $"{SubPath}/{id.Key})");

    public object? GetItem(RegKey id) => parentRegister.GetItem(GetFullId(id));

    public bool ItemRegistered(RegKey id) => parentRegister.ItemRegistered(GetFullId(id));

    public object? this[RegKey id] => GetItem(id);


    // Static

    private static readonly Dictionary<(IRegister<RegKey> parentRegister, string subPath), SubRegister> CacheDict = new();

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

    public static SubRegister Get(IRegister<RegKey> parentRegister, string subPath)
    {
        var t = (parentRegister, subPath);
        if (!CacheDict.TryGetValue(t, out var reg))
            CacheDict[t] = reg = new SubRegister(parentRegister, subPath);
        return reg;
    }
}

public class SubRegister<TItem>(IRegister<RegKey, TItem> parentRegister, string subPath) : SubRegister(parentRegister, subPath), IRegister<RegKey, TItem>
{
    public new TItem GetItem(RegKey id) => parentRegister.GetItem(GetFullId(id));

    public new bool ItemRegistered(RegKey id) => parentRegister.ItemRegistered(GetFullId(id));

    public new TItem this[RegKey id] => parentRegister[GetFullId(id)];
}