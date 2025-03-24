using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using BabelRush.Data;
using BabelRush.Registers;

using Godot;

using Tomlyn;

namespace BabelRush.Gui.Misc;

public record FontInfo(RegKey FontId, int Size)
{
    public Font Font => LocalInfoRegisters.Fonts[FontId];
}

[Model]
internal partial class FontInfoModel : IResModel<FontInfo>
{
    [IgnoreDataMember]
    public string Id { get; private set; } = "";

    [NecessaryProperty]
    public partial string Font { get; set; }

    [NecessaryProperty]
    public partial int Size { get; set; }

    public (RegKey, FontInfo) Convert(string nameSpace)
    {
        RegKey id = (nameSpace, Id);
        RegKey font = Font.WithDefaultNameSpace(nameSpace);
        var info = new FontInfo(font, Size);
        return (id, info);
    }


    public static IReadOnlyCollection<IModel<FontInfo>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        if (!source.Files.TryGetValue(".toml", out var toml))
        {
            errorMessages = new(1, ["toml file not found"]);
            return [];
        }

        Toml.Parse(toml).TryToModel<FontInfoModel>(out var model, out var diagnostics);
        var errors = diagnostics.Select(msg => msg.ToString()).ToList();

        if (model is null)
        {
            errorMessages = new(errors.Count, errors.ToArray());
            return [];
        }
        if (!model.Check(out var errs))
        {
            errors.AddRange(errs);
            errorMessages = new(errors.Count, errors.ToArray());
            return [];
        }

        errorMessages = new(errors.Count, errors.ToArray());
        model.Id = source.Path;
        return [model];
    }
}