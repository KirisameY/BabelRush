using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Registers;

using Godot;

namespace BabelRush.Mobs.Animation;

public class MobAnimationSetBuilder
{
    private string? Id { get; set; }
    private SpriteFrames SpriteFrames { get; } = new();
    private Dictionary<MobAnimationId, MobAnimationSet.AnimationInfo> AnimationDict { get; } = [];

    public MobAnimationSetBuilder SetId(string id)
    {
        Id = id;
        return this;
    }

    public MobAnimationSetBuilder SetAnimation
    (MobAnimationId id, IEnumerable<(Texture2D, float)> frames, Vector2I center, Vector2I boxSize,
     float fps = 5.0f, MobAnimationId? beforeAnimation = null, MobAnimationId? afterAnimation = null)
    {
        if (!id.IsAction && (beforeAnimation, afterAnimation) is not (null, null))
            throw new InvalidOperationException("State animation cannot have before and after animations");

        if (fps < 0)
        {
            fps = -fps;
            frames = frames.Reverse();
        }

        StringName name = new(id);
        if (SpriteFrames.HasAnimation(name)) SpriteFrames.RemoveAnimation(name);
        SpriteFrames.AddAnimation(name);
        SpriteFrames.SetAnimationSpeed(name, fps);
        SpriteFrames.SetAnimationLoop(name, !id.IsAction);
        foreach (var (frame, duration) in frames)
            SpriteFrames.AddFrame(name, frame, duration);
        AnimationDict[id] = new(-center, boxSize, beforeAnimation, afterAnimation);
        return this;
    }

    public MobAnimationSetBuilder SetAnimation
    (MobAnimationId id, IEnumerable<Texture2D> frames, Vector2I center, Vector2I boxSize,
     float fps = 5.0f, MobAnimationId? beforeAnimation = null, MobAnimationId? afterAnimation = null) =>
        SetAnimation(id, frames.Select(frame => (frame, 1.0f)), center, boxSize, fps, beforeAnimation, afterAnimation);

    public MobAnimationSetBuilder SetAnimation(MobAnimationModel model) =>
        throw new NotImplementedException();

    public MobAnimationSet Build()
    {
        if (Id is null) throw new InvalidOperationException("Id is not set");
        if (!AnimationDict.ContainsKey(MobAnimationId.Default))
            throw new InvalidOperationException($"Animation \"{MobAnimationId.Default}\" does not exist");
        return new(Id, SpriteFrames, AnimationDict);
    }
}