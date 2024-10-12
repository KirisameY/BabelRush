using System;
using System.Collections.Generic;

using BabelRush.Registering.Parsing;

using KirisameLib.Core.Extensions;

namespace BabelRush.Data;

public class NameDescBox(string id, NameDesc nameDesc) : ILangBox<NameDesc, NameDescBox>
{
    public string Id { get; } = id;

    public NameDesc GetAsset() => nameDesc;

    public static NameDescBox FromEntry(KeyValuePair<string, object> entry)
    {
        var data = (IDictionary<string, object>)entry.Value;
        var name = Convert.ToString(data["name"]); // todo: 用 convert 取代所以Box类的强转, just like this
        var desc = data.GetOrDefault("desc") as string ?? "";
        var quote = data.GetOrDefault("quote") as string ?? "";
        return new(entry.Key, new(name, desc, quote));
    }
}