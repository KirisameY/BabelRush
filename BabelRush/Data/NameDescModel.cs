using System.Collections.Generic;

using KirisameLib.Core.Extensions;
using KirisameLib.Data.Model;

namespace BabelRush.Data;

public class NameDescModel(string id, NameDesc nameDesc) : ILangModel<NameDesc>
{
    public string Id { get; } = id;

    public NameDesc Convert() => nameDesc;

    public static NameDescModel FromEntry(KeyValuePair<string, object> entry)
    {
        var data = (IDictionary<string, object>)entry.Value;
        var name = (string)data["name"];
        var desc = System.Convert.ToString(data.GetOrDefault("desc")) ?? "";
        var quote = System.Convert.ToString(data.GetOrDefault("quote")) ?? "";
        return new(entry.Key, new(name, desc, quote));
    }

    public static IModel<NameDesc>[] FromSource(IDictionary<string, object> source, out ModelParseErrorInfo errorMessages)
    {
        throw new System.NotImplementedException();
    }
}