using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using Godot;

using KirisameLib.Extensions;

namespace BabelRush.Mobs.Animation;

internal class MobAnimationSetBuilder(string id)
{
    private SpriteFrames SpriteFrames { get; } = new();
    private Dictionary<MobAnimationId, MobAnimationSet.AnimationInfo> AnimationDict { get; } = [];

    public MobAnimationSetBuilder AddAnimation(
        MobAnimationId animationId, IEnumerable<Texture2D> frames, Vector2I center, Vector2I boxSize, float fps = 5.0f,
        Dictionary<int, float>? timeScales = null, MobAnimationId? beforeAnimation = null, MobAnimationId? afterAnimation = null)
    {
        if (!animationId.IsAction && (beforeAnimation, afterAnimation) is not (null, null))
            throw new InvalidOperationException("State animation cannot have before and after animations");

        if (fps < 0)
        {
            fps = -fps;
            frames = frames.Reverse();
        }

        StringName name = new(animationId);
        if (SpriteFrames.HasAnimation(name)) SpriteFrames.RemoveAnimation(name);
        SpriteFrames.AddAnimation(name);
        SpriteFrames.SetAnimationSpeed(name, fps);
        SpriteFrames.SetAnimationLoop(name, !animationId.IsAction);
        timeScales ??= [];
        frames.Select((frame, index) => (timeScale: timeScales.GetValueOrDefault(index, 1.0f), frame))
              .ForEach(t => SpriteFrames.AddFrame(name, t.frame, t.timeScale));
        AnimationDict[animationId] = new(-center, boxSize, beforeAnimation, afterAnimation);
        return this;
    }

    public MobAnimationSetBuilder AddAnimation(MobAnimationModel model)
    {
        if (model.MobId != id) throw new InvalidOperationException("Model is not for this animation set");
        return AddAnimation(model.AnimationId,
                            DataUtils.CutAtlasTexture(model.FrameAtlas, model.Columns, model.Rows),
                            model.FrameCenter, model.BoxSize, model.Fps,
                            model.FrameTimeScale,
                            model.BeforeAnimation, model.AfterAnimation);
    }

    public MobAnimationSet Build()
    {
        if (!AnimationDict.ContainsKey(MobAnimationId.Default))
            throw new InvalidOperationException($"Animation \"{MobAnimationId.Default}\" does not exist");
        return new(id, SpriteFrames, AnimationDict);
    }
}