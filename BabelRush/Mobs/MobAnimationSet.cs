using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

using Godot;


namespace BabelRush.Mobs;

public class MobAnimationSet(
    SpriteFrames spriteFrames, IDictionary<MobAnimationId, MobAnimationSet.AnimationInfo> animationDict, MobAnimationId defaultId)
{
    #region Properties

    public SpriteFrames SpriteFrames { get; } = spriteFrames;
    private FrozenDictionary<MobAnimationId, AnimationInfo> AnimationDict { get; } = animationDict.ToFrozenDictionary();
    public MobAnimationId DefaultId { get; } = defaultId;

    #endregion


    #region Public Methods

    public AnimationInfo this[MobAnimationId id] => AnimationDict[id];

    public bool HasAnimation(MobAnimationId id) => AnimationDict.ContainsKey(id);

    public bool TryGetInfo(MobAnimationId id, out AnimationInfo info) => AnimationDict.TryGetValue(id, out info);

    public MobAnimationId BackToExist(MobAnimationId id, out AnimationInfo info)
    {
        foreach (var backId in id.Backoff())
        {
            if (TryGetInfo(backId, out info)) return backId;
        }
        info = this[DefaultId];
        return DefaultId;
    }

    #endregion


    //Entry Info
    public readonly struct AnimationInfo(Vector2I offset, Vector2I size, MobAnimationId? start = null, MobAnimationId? end = null)
    {
        public readonly Vector2I Offset = offset;
        public readonly Vector2I Size = size;

        public readonly MobAnimationId? Start = start;
        public readonly MobAnimationId? End = end;
    }
}