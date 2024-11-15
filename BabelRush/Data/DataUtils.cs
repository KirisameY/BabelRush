using System;
using System.Collections.Generic;

using Godot;

namespace BabelRush.Data;

public static class DataUtils
{
    public static Image LoadImageFromPngBuffer(byte[] buffer)
    {
        var image = new Image();
        image.LoadPngFromBuffer(buffer);
        return image;
    }
}