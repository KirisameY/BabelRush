using System.Collections.Frozen;
using System.Collections.Generic;

using Godot;

namespace BabelRush.Mobs.Animation;

public class MobAnimationSet(
    string id,
    SpriteFrames spriteFrames,
    IDictionary<MobAnimationId, MobAnimationSet.AnimationInfo> animationDict
    // MobAnimationId defaultAnimationId
)
{
    #region Properties

    public string Id { get; } = id;
    public SpriteFrames SpriteFrames { get; } = spriteFrames;
    private FrozenDictionary<MobAnimationId, AnimationInfo> AnimationDict { get; } = animationDict.ToFrozenDictionary();
    // public MobAnimationId DefaultAnimationId { get; } = defaultAnimationId;

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
        info = this[MobAnimationId.Default];
        return MobAnimationId.Default;
    }

    #endregion


    #region Static

    private static MobAnimationSet? _default;

    public static MobAnimationSet Default => _default ??=
        new MobAnimationSetBuilder("default")
           .AddAnimation("idle", [new PlaceholderTexture2D { Size = new(48, 64) }], new(24, 64), new(48, 64))
           .Build();

    #endregion


    //Entry Info
    public readonly struct AnimationInfo(Vector2I offset, Vector2I boxSize, MobAnimationId? start = null, MobAnimationId? end = null)
    {
        public readonly Vector2I Offset = offset;
        public readonly Vector2I BoxSize = boxSize;

        public readonly MobAnimationId? Start = start;
        public readonly MobAnimationId? End = end;
    }
}