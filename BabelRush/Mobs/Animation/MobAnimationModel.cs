using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;

using BabelRush.Data;
using BabelRush.Data.ExtendModels;

using Godot;

using KirisameLib.Extensions;

using Tomlyn;

namespace BabelRush.Mobs.Animation;

[Model]
internal partial class MobAnimationModel : IResModel<MobAnimationModel>
{
    [IgnoreDataMember]
    public RegKey Id => (SetId.NameSpace, $"{SetId.Key}/{AnimationId}");

    [IgnoreDataMember]
    public RegKey SetId { get; private set; } = RegKey.Default;
    [IgnoreDataMember]
    public string AnimationId { get; private set; } = "";

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
        get => field ??= new PlaceholderTexture2D { Size = new(24, 24) };
        set;
    }

    public Dictionary<int, float> FrameTimeScale { get; set; } = [];
    public string? BeforeAnimation { get; set; }
    public string? AfterAnimation { get; set; }


    public (RegKey, MobAnimationModel) Convert(string nameSpace, string path)
    {
        //todo: 这个倒也好办，回头重置完MobAnimation之后把它做成MobAnimationEntry的Model即可
        throw new System.NotImplementedException();
    }


    public static IReadOnlyCollection<IModel<MobAnimationModel>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        List<string> errors = [];

        //check file existence
        if (!source.Files.TryGetValue(".toml", out var tomlFile)) errors.Add("Missing toml configuration file");
        if (!source.Files.TryGetValue(".png",  out var pngFile)) errors.Add("Missing png resource file");
        if (source.Dir.Length == 0) errors.Add("MobAnimation in root directory will not be loaded");

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
            errors.AddRange(checkErrors);
            errorMessages = new(errors.Count, errors.ToArray());
            return [];
        }

        errorMessages = new(errors.Count, errors.ToArray());
        model.FrameAtlas = ImageTexture.CreateFromImage(DataUtils.LoadImageFromPngBuffer(pngFile!));

        model.SetId = source.Dir.Join('/');
        model.AnimationId = source.Name;
        return [model];
    }

    public static MobAnimationModel Default { get; } = new()
    {
        SetId = RegKey.Default,
        AnimationId = "default",
        Columns = 1,
        Rows = 1,
        FrameCenter = new() { X = 12, Y = 0 },
        BoxSize = new() { X = 24, Y = 24 },
        Fps = 0,
        FrameAtlas = new PlaceholderTexture2D { Size = new(24, 24) }
    };
}