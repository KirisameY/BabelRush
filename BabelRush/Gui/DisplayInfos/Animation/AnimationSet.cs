using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Data;

using Godot;

namespace BabelRush.Gui.DisplayInfos.Animation;

public class AnimationSet(
    RegKey id, SpriteFrames spriteFrames,
    IDictionary<AnimationId, AnimationSet.AnimationInfo> animationDict
    // MobAnimationId defaultAnimationId
)
{
    #region Properties

    public RegKey Id { get; } = id;
    public SpriteFrames SpriteFrames { get; } = spriteFrames;
    private FrozenDictionary<AnimationId, AnimationInfo> AnimationDict { get; } = animationDict.ToFrozenDictionary();
    // public MobAnimationId DefaultAnimationId { get; } = defaultAnimationId;

    #endregion


    #region Public Methods

    public AnimationInfo this[AnimationId id] => AnimationDict[id];

    public bool HasAnimation(AnimationId id) => AnimationDict.ContainsKey(id);

    public bool TryGetInfo(AnimationId id, out AnimationInfo info) => AnimationDict.TryGetValue(id, out info);

    public AnimationId BackToExist(AnimationId id, out AnimationInfo info)
    {
        foreach (var backId in id.Backoff())
        {
            if (TryGetInfo(backId, out info)) return backId;
        }
        info = this[AnimationId.Default];
        return AnimationId.Default;
    }

    #endregion


    #region Static

    [field: AllowNull, MaybeNull]
    public static AnimationSet Default => field ??=
        new AnimationSetBuilder(RegKey.Default)
           .AddAnimation("idle", [new PlaceholderTexture2D { Size = new(48, 48) }], new(24, 48), new(48, 48))
           .Build();

    #endregion


    //Entry Info
    public readonly struct AnimationInfo(Vector2I offset, Vector2I boxSize, AnimationId? before = null, AnimationId? after = null)
    {
        public readonly Vector2I Offset = offset;
        public readonly Vector2I BoxSize = boxSize;

        public readonly AnimationId? Before = before;
        public readonly AnimationId? After = after;
    }
}