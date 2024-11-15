using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;
using BabelRush.Data.ExtendModels;

using Godot;

using JetBrains.Annotations;

using KirisameLib.Data.Model;

namespace BabelRush.Mobs.Animation;

//todo:把这个反序列化做完
// public record MobAnimationSetModel(string Id, string DefaultAnimationId, ImmutableArray<MobAnimationSetModel.MobAnimationData> Animations) :
//     IResModel<MobAnimationSet>
// {
//     public MobAnimationSet Convert()
//     {
//         throw new NotImplementedException();
//
//         // var builder = new MobAnimationSetBuilder().SetId(Id)
//         //                                           .SetDefault(DefaultAnimationId);
//         // foreach (var a in Animations)
//         // {
//         //     dynamic frames;
//         //     if (a.FrameTimes is null)
//         //         frames = a.Frames.Select(SomeMagicToGetATexture2D);
//         //     else
//         //         frames = a.Frames.Select((f, i) => (SomeMagicToGetATexture2D(f), a.FrameTimes.GetOrDefault(i, 1f)));
//         //     builder.SetAnimation(a.Id, frames, a.Center, a.BoxSize, a.Fps,
//         //                          a.BeforeAnimation is not null ? MobAnimationId.Get(a.BeforeAnimation) : null,
//         //                          a.AfterAnimation is not null ? MobAnimationId.Get(a.AfterAnimation) : null);
//         // }
//         // return builder.Build();
//     }
//
//
//     private static MobAnimationSetModel FromEntry(IDictionary<string, object> entry)
//     {
//         var id = (string)entry["id"];
//         var defaultAnimation = (string)entry["default_animation"];
//         var animations = ((IList<IDictionary<string, object>>)entry["animations"]).Select(MobAnimationData.FromDataEntry);
//         return new(id, defaultAnimation, [..animations]);
//     }
//
//
//     public static MobAnimationSetModel? FromEntry(ResSourceInfo entry)
//     {
//         throw new NotImplementedException();
//         // if (entry.Data is null) return null;
//         // return FromEntry(entry.Data);
//     }
//
//     public static IModel<MobAnimationSet>[] FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
//     {
//         throw new NotImplementedException();
//     }
//
//
//     #region Sub Classes
//
//
//
//     #endregion
// }

[Model]
public partial class MobAnimationModel : IResModel<MobAnimationModel>
{
    [NecessaryProperty]
    public partial string Id { get; set; }
    [NecessaryProperty]
    public partial int Frames { get; set; }
    [NecessaryProperty]
    public partial Vector2IModel Center { get; set; }
    [NecessaryProperty]
    public partial Vector2IModel BoxSize { get; set; }
    [NecessaryProperty]
    public partial float Fps { get; set; }
    
    public Dictionary<int, float> FrameTimes { get; set; } = [];
    public string? BeforeAnimation { get; set; }
    public string? AfterAnimation { get; set; }

    // public static MobAnimationModel FromDataEntry(IDictionary<string, object> entry)
    // {
    //     var id = (string)entry["id"];
    //     var frames = System.Convert.ToInt32(entry["frames"]);
    //     var center = DataUtils.GetVector2I((IDictionary<string, object>)entry["center"]);
    //     var boxSize = DataUtils.GetVector2I((IDictionary<string, object>)entry["box_size"]);
    //     var fps = System.Convert.ToSingle(entry["fps"]);
    //
    //     entry.TryGetValue("frame_times", out var oFrameTimes1);
    //     var oFrameTimes = oFrameTimes1 as IDictionary<string, object>;
    //     var frameTimes = oFrameTimes?.Aggregate
    //         (new Dictionary<int, float>(),
    //          (dict, pair) =>
    //          {
    //              int i = int.Parse(pair.Key);
    //              float f = System.Convert.ToSingle(pair.Value);
    //              dict[i] = f;
    //              return dict;
    //          },
    //          dict => dict.ToFrozenDictionary()
    //         );
    //     entry.TryGetValue("before_animation", out var oBeforeAnimation);
    //     var beforeAnimation = oBeforeAnimation as string;
    //     entry.TryGetValue("after_animation", out var oAfterAnimation);
    //     var afterAnimation = oAfterAnimation as string;
    //
    //     return new(id, frames, frameTimes, center, boxSize, fps, beforeAnimation, afterAnimation);
    // }

    public MobAnimationModel Convert() => this;

    public static IReadOnlyCollection<IModel<MobAnimationModel>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        List<string> errors = [];

        //check file existence
        if (!source.Files.TryGetValue("toml", out var tomlFile)) errors.Add("Missing toml configuration file");
        if (!source.Files.TryGetValue("png",  out var pngFile)) errors.Add("Missing png configuration file");
        if (errors.Count > 0)
        {
            errorMessages = new ModelParseErrorInfo(errors.Count, errors.ToArray());
            return [];
        }

        throw new NotImplementedException();
    }
}