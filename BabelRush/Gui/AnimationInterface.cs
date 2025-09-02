using System.Diagnostics.CodeAnalysis;

using BabelRush.Gui.DisplayInfos.Animation;
using BabelRush.Utils;

using Godot;

using KirisameLib.Asynchronous.SyncTasking;
using KirisameLib.Logging;

namespace BabelRush.Gui;

public abstract partial class AnimationInterface : Node2D
{
    [field: AllowNull, MaybeNull]
    protected AnimationId AnimateState
    {
        get { return field ??= AnimationId.Default; }
        set;
    }

    protected abstract AnimationSet AnimationSet { get; }

    protected abstract AnimatedSprite2D Sprite { get; }
    protected abstract CollisionShape2D? BoxShapeNode { get; }
    protected abstract RectangleShape2D? BoxShape { get; }

    [LogToSync]
    protected async SyncTask PlayAnimationAsync(AnimationId id)
    {
        var animationSet = AnimationSet;
        id = animationSet.BackToExist(id, out var info);

        //case state
        if (!id.IsAction)
        {
            AnimateState = id;
            PlayAnim(this, id, info);
            return;
        }

        //case action
        //play before
        if (info.Before is not null)
        {
            await PlayAnimationAsync(info.Before);
        }

        //Play this
        PlayAnim(this, id, info);
        await ToSignal(Sprite, AnimatedSprite2D.SignalName.AnimationFinished);

        //play after
        if (info.After is not null)
        {
            await PlayAnimationAsync(info.After);
        }

        //reset
        _ = PlayAnimationAsync(AnimateState);

        return;


        static void PlayAnim(AnimationInterface obj, AnimationId aId, AnimationSet.AnimationInfo aInfo)
        {
            obj.BoxShape?.Size         = aInfo.BoxSize;
            obj.BoxShapeNode?.Position = new(0, -aInfo.BoxSize.Y / 2f);
            obj.Sprite.Offset          = aInfo.Offset;
            obj.Sprite.Play(aId);
        }
    }


    // Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(AnimationInterface));
}