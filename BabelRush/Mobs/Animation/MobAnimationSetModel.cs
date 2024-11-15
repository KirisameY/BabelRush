using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

using BabelRush.Data;
using BabelRush.Data.ExtendModels;

using Godot;

using KirisameLib.Data.Model;

using Tomlyn;

namespace BabelRush.Mobs.Animation;

[Model]
internal partial class MobAnimationModel : IResModel<MobAnimationModel>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial int Columns { get; set; }
    [NecessaryProperty]
    public partial int Rows { get; set; }
    [NecessaryProperty]
    public partial Vector2IModel FrameCenter { get; set; }
    [NecessaryProperty]
    public partial Vector2IModel BoxSize { get; set; }
    [NecessaryProperty]
    public partial float Fps { get; set; }

    [IgnoreDataMember]
    [field: AllowNull, MaybeNull]
    public Texture2D FrameAtlas
    {
        get => field ?? new PlaceholderTexture2D();
        set;
    }

    public Dictionary<int, float> FrameTimeScale { get; set; } = [];
    public string? BeforeAnimation { get; set; }
    public string? AfterAnimation { get; set; }
    

    public MobAnimationModel Convert() => this;
    
    public static IReadOnlyCollection<IModel<MobAnimationModel>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        List<string> errors = [];

        //check file existence
        if (!source.Files.TryGetValue("toml", out var tomlFile)) errors.Add("Missing toml configuration file");
        if (!source.Files.TryGetValue("png",  out var pngFile)) errors.Add("Missing png resource file");
        if (errors.Count > 0)
        {
            errorMessages = new ModelParseErrorInfo(errors.Count, errors.ToArray());
            return [];
        }

        Toml.Parse(tomlFile!).TryToModel(out MobAnimationModel? model, out var diagnostics);
        errors.AddRange(diagnostics.Select(static diagnostic => diagnostic.ToString()));
        if (model is null)
        {
            errorMessages = new(errors.Count, errors.ToArray());
            return [];
        }
        if (!model.Check(out var checkErrors))
        {
            errorMessages = new(errors.Count, errors.ToArray());
            errors.AddRange(checkErrors);
        }

        model.FrameAtlas = ImageTexture.CreateFromImage(DataUtils.LoadImageFromPngBuffer(pngFile!));
        errorMessages = new(errors.Count, errors.ToArray());
        return [model];
    }
}