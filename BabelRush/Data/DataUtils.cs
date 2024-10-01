using System;
using System.Collections.Generic;

using Godot;

namespace BabelRush.Data;

public static class DataUtils
{
    public static IEnumerable<ParseResult<TData>> FromTableList<TData>(IDictionary<string, object> table, string listKey)
        where TData : notnull, IData<TData>
    {
        if (!table.TryGetValue(listKey, out var item)) yield break;
        if (item is not IEnumerable<IDictionary<string, object>> list) yield break;
        foreach (var entry in list)
        {
            ParseResult<TData> result;
            try
            {
                result = new(TData.FromEntry(entry));
            }
            catch (Exception e)
            {
                result = new(e);
            }
            yield return result;
        }
    }

    public static Vector2I GetVector2I(IList<int> data)
    {
        return new(Convert.ToInt32(data[0]), Convert.ToInt32(data[1]));
    }
}