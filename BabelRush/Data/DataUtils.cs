using System;
using System.Collections.Generic;

using Godot;

namespace BabelRush.Data;

public static class DataUtils
{
    public static Vector2I GetVector2I(IDictionary<string, object> data)
    {
        return new(Convert.ToInt32(data["x"]), Convert.ToInt32(data["y"]));
    }

    public static Vector2 GetVector2(IDictionary<string, object> data)
    {
        return new(Convert.ToSingle(data["x"]), Convert.ToSingle(data["y"]));
    }

    public static Image LoadImageFromPngBuffer(byte[] buffer)
    {
        var image = new Image();
        image.LoadPngFromBuffer(buffer);
        return image;
    }
}