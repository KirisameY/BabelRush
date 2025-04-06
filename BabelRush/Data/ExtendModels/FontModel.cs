using System.Collections.Generic;
using System.Linq;

using Godot;

using Tomlyn;

namespace BabelRush.Data.ExtendModels;

public class FontModel(string id, Font font) : IResModel<Font>
{
    public string Id => id;

    public Font Convert() => font;
    public (RegKey, Font) Convert(string nameSpace) => ((nameSpace, id), font);

    public static IReadOnlyCollection<IModel<Font>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        Font font;
        // todo: 有机会的话我应该给它做一个方便的获取文件的方法，比如传入需要的后缀的信息（以及是否必要等），然后传出一个字典
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
            return [new FontModel(source.Path, font)];
        }

        Toml.Parse(toml).TryToModel<FontVariation>(out var fontVariation, out var diagnostics);
        errorMessages = new ModelParseErrorInfo(diagnostics.Count, diagnostics.Select(msg => msg.ToString()).ToArray());
        if (fontVariation is null) return [];

        fontVariation.BaseFont = font;
        return [new FontModel(source.Path, fontVariation)];
    }
}