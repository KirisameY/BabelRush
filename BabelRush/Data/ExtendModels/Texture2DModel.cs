using System.Collections.Generic;

using Godot;

using KirisameLib.Data.Model;

namespace BabelRush.Data.ExtendModels;

public class Texture2DModel(string id, Texture2D texture) : IResModel<Texture2D>
{
    public string Id { get; } = id;

    public Texture2D Convert() => texture;

    public static IReadOnlyCollection<IModel<Texture2D>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        if (!source.Files.TryGetValue(".png", out var file))
        {
            errorMessages = new(1, ["png file not found"]);
            return [];
        }

        errorMessages = ModelParseErrorInfo.Empty;
        var image = DataUtils.LoadImageFromPngBuffer(file);
        var tex = ImageTexture.CreateFromImage(image);
        return [new Texture2DModel(source.Id, tex)];
    }
}