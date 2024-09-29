using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Data;

using Godot;

using KirisameLib.Core.Extensions;

using Tomlyn.Model;

namespace BabelRush.Mobs.Animation;

public record MobAnimationSetData(string Id, string DefaultAnimationId, ImmutableArray<MobAnimationData> Animations)
    : ITomlData<MobAnimationSetData>
{
    public MobAnimationSet ToMobAnimationSet()
    {
        var builder = new MobAnimationSetBuilder().SetId(Id)
                                                  .SetDefault(DefaultAnimationId);
        foreach (var a in Animations)
        {
            dynamic frames;
            if (a.FrameTimes is null)
                frames = a.Frames.Select(SomeMagicToGetATexture2D);
            else
                frames = a.Frames.Select((f, i) => (SomeMagicToGetATexture2D(f), a.FrameTimes.GetOrDefault(i, 1f)));
            builder.SetAnimation(a.Id, frames, a.Center, a.BoxSize, a.Fps,
                                 a.BeforeAnimation is not null ? MobAnimationId.Get(a.BeforeAnimation) : null,
                                 a.AfterAnimation is not null ? MobAnimationId.Get(a.AfterAnimation) : null);
        }
        return builder.Build();
    }

    private Texture2D SomeMagicToGetATexture2D(string maybeAPath)
    {
        throw new NotImplementedException();
    }

    public static MobAnimationSetData FromTomlEntry(TomlTable entry)
    {
        var id = (string)entry["id"];
        var defaultAnimation = (string)entry["default_animation"];
        var animations = DataUtils.FromTomlTable<MobAnimationData>(entry, "animations").Select(result => result.Result);
        return new(id, defaultAnimation, [..animations]);
    }
}

public record MobAnimationData(
    string Id, ImmutableArray<string> Frames, FrozenDictionary<int, float>? FrameTimes, Vector2I Center, Vector2I BoxSize,
    float Fps, string? BeforeAnimation, string? AfterAnimation) : ITomlData<MobAnimationData>
{
    public static MobAnimationData FromTomlEntry(TomlTable entry)
    {
        var id = (string)entry["id"];
        var frames = ((TomlArray)entry["frames"]).Select(o => (string)o!);
        var center = DataUtils.GetVector2I((TomlTable)entry["center"]);
        var boxSize = DataUtils.GetVector2I((TomlTable)entry["box_size"]);
        var fps = Convert.ToSingle(entry["fps"]);

        entry.TryGetValue("frame_times", out var oFrameTimes1);
        var oFrameTimes = oFrameTimes1 as TomlTable;
        var frameTimes = oFrameTimes?.Aggregate
            (new Dictionary<int, float>(),
             (dict, pair) =>
             {
                 int i = int.Parse(pair.Key);
                 float f = Convert.ToSingle(pair.Value);
                 dict[i] = f;
                 return dict;
             },
             dict => dict.ToFrozenDictionary()
            );
        entry.TryGetValue("before_animation", out var oBeforeAnimation);
        var beforeAnimation = oBeforeAnimation as string;
        entry.TryGetValue("after_animation", out var oAfterAnimation);
        var afterAnimation = oAfterAnimation as string;

        return new(id, [..frames], frameTimes, center, boxSize, fps, beforeAnimation, afterAnimation);
    }
}