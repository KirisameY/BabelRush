using System;
using System.Collections.Generic;

using Godot;

using Tomlyn.Model;

namespace BabelRush.Data;

public static class DataUtils
{
    public static IEnumerable<ParseResult<TData>> FromTomlTable<TData>(TomlTable table, string listKey)
        where TData : notnull, ITomlData<TData>
    {
        if (!table.TryGetValue(listKey, out var item)) yield break;
        if (item is not TomlTableArray list) yield break;
        foreach (var entry in list)
        {
            ParseResult<TData> result;
            try
            {
                result = new(TData.FromTomlEntry(entry));
            }
            catch (Exception e)
            {
                result = new(e);
            }
            yield return result;
        }
    }
    
    public static Vector2I GetVector2I(TomlTable table)
    {
        return new(Convert.ToInt32(table["x"]), Convert.ToInt32(table["y"]));
    }
}