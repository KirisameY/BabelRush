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

    public static AtlasTexture[] CutAtlasTexture(Texture2D source, int columns, int rows)
    {
        var sourceSize = source.GetSize();
        var w = sourceSize.X / columns;
        var h = sourceSize.Y / rows;
        var result = new AtlasTexture[columns * rows];
        var index = 0;
        for (int r = 0; r < rows; r++)
        {
            for (int c = 0; c < columns; c++)
            {
                var rect = new Rect2(c * w, r * h, w, h);
                result[index++] = new AtlasTexture
                {
                    Atlas = source,
                    Region = rect
                };
            }
        }
        return result;
    }
}