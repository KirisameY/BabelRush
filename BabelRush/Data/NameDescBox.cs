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
        var name = (string)data["name"];
        var desc = Convert.ToString(data.GetOrDefault("desc")) ?? "";
        var quote = Convert.ToString(data.GetOrDefault("quote")) ?? "";
        return new(entry.Key, new(name, desc, quote));
    }
}