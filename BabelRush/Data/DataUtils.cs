using System;
using System.Collections.Generic;

using BabelRush.Misc;

using Godot;

namespace BabelRush.Data;

public static class DataUtils
{
    public static Vector2I GetVector2I(IDictionary<string, object> data)
    {
        return new(Convert.ToInt32(data["x"]), Convert.ToInt32(data["y"]));
    }

    public static (string Id, NameDesc NameDesc) ParseNameDescAndId(KeyValuePair<string, object> entry) =>
        (entry.Key, NameDesc.FromEntry((IDictionary<string, object>)entry.Value));
}