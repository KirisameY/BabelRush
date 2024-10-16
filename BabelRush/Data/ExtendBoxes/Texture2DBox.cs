using System;

using BabelRush.Registering.Parsing;

using Godot;

namespace BabelRush.Data.ExtendBoxes;

public class Texture2DBox(string id, Texture2D texture) : IResBox<Texture2D, Texture2DBox>
{
    public string Id { get; } = id;

    public Texture2D GetAsset() => texture;

    public static Texture2DBox? FromEntry(ResSource entry)
    {
        if (!entry.Files.TryGetValue("png", out var file)) return null;
        var buffer = file.GetBuffer((long)file.GetLength());

        var image = DataUtils.LoadImageFromPngBuffer(buffer);
        var tex = ImageTexture.CreateFromImage(image);
        return new Texture2DBox(entry.Id, tex);
    }
}