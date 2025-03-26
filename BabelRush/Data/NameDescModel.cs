using System.Collections.Generic;

using KirisameLib.Extensions;

namespace BabelRush.Data;

internal class NameDescModel(string id, NameDesc nameDesc) : ILangModel<NameDesc>
{
    public string Id => id;

    public (RegKey, NameDesc) Convert(string nameSpace) => ((nameSpace, id), nameDesc);

    public static NameDescModel? FromEntry(KeyValuePair<string, object> entry, out string? error)
    {
        if (entry.Value is not IDictionary<string, object> data)
        {
            error = "Source is not a valid NameDesc table";
            return null;
        }

        if (data["name"] is not string name)
        {
            error = "Necessary field 'name' is missing or not a string";
            return null;
        }

        error = null;
        var desc = System.Convert.ToString(data.GetOrDefault("desc")) ?? "";
        var quote = System.Convert.ToString(data.GetOrDefault("quote")) ?? "";
        return new(entry.Key, new(name, desc, quote));
    }

    public static IReadOnlyCollection<IModel<NameDesc>> FromSource(
        IDictionary<string, object> source, out ModelParseErrorInfo errorMessages)
    {
        List<NameDescModel> models = [];
        List<string> errors = [];
        foreach (var item in source)
        {
            if (FromEntry(item, out var error) is { } model) models.Add(model);
            else errors.Add(error!);
        }
        errorMessages = new(errors.Count, errors.ToArray());
        return models;
    }
}