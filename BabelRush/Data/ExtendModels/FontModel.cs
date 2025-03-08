using System.Collections.Generic;
using System.Linq;

using Godot;

using Tomlyn;

namespace BabelRush.Data.ExtendModels;

public class FontModel(string id, Font font) : IResModel<Font>
{
    public string Id => id;

    public Font Convert() => font;


    public static IReadOnlyCollection<IModel<Font>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        Font font;
        switch (source.Files.TryGetValue(".ttf", out var ttf), source.Files.TryGetValue(".otf", out var otf))
        {
            case (true, true):
                errorMessages = new(1, ["both otf and ttf file found"]);
                return [];
            case (false, false):
                errorMessages = new(1, ["neither otf nor ttf file found"]);
                return [];
            case (true, false):
                font = new FontFile { Data = ttf };
                break;
            case (false, true):
                font = new FontFile { Data = otf };
                break;
        }

        if (!source.Files.TryGetValue(".toml", out var toml))
        {
            errorMessages = ModelParseErrorInfo.Empty;
            return [new FontModel(source.Id, font)];
        }

        Toml.Parse(toml).TryToModel<FontVariation>(out var fontVariation, out var diagnostics);
        errorMessages = new ModelParseErrorInfo(diagnostics.Count, diagnostics.Select(msg => msg.ToString()).ToArray());
        if (fontVariation is null) return [];

        fontVariation.BaseFont = font;
        return [new FontModel(source.Id, fontVariation)];
    }
}