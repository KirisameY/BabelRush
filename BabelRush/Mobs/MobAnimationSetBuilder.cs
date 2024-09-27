using System;
using System.Collections.Generic;
using System.Linq;

using Godot;

namespace BabelRush.Mobs;

public class MobAnimationSetBuilder
{
    private SpriteFrames SpriteFrames { get; } = new();
    private Dictionary<MobAnimationId, MobAnimationSet.AnimationInfo> AnimationDict { get; } = [];
    private MobAnimationId? DefaultId { get; set; }

    public MobAnimationSetBuilder AddAnimation
        (MobAnimationId id, IEnumerable<(Texture2D, float)> frames, Vector2I offset, Vector2I size, float speed = 1.0f)
    {
        StringName name = new(id);
        SpriteFrames.AddAnimation(name);
        SpriteFrames.SetAnimationSpeed(name, speed);
        foreach (var (frame, duration) in frames)
            SpriteFrames.AddFrame(name, frame, duration);
        AnimationDict.Add(id, new(offset, size));
        return this;
    }

    public MobAnimationSetBuilder AddAnimation
        (MobAnimationId name, IEnumerable<Texture2D> frames, Vector2I offset, Vector2I size, float speed = 1.0f) =>
        AddAnimation(name, frames.Select((frame) => (frame, 1.0f)), offset, size, speed);

    public MobAnimationSetBuilder SetDefault(MobAnimationId name)
    {
        DefaultId = name;
        return this;
    }

    public MobAnimationSet Build()
    {
        if (DefaultId is null) throw new InvalidOperationException("Default animation is not set");
        if (!AnimationDict.ContainsKey(DefaultId)) throw new InvalidOperationException("Default animation does not exist");
        return new(SpriteFrames, AnimationDict, DefaultId);
    }
}