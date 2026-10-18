using System;
using System.Collections.Generic;

using BabelRushR.Game.GUI.Cards;

using Godot;

namespace BabelRushR.Game.GUI.Hand;

/// <summary>
///     <see cref="HandCardsUI"/> 的布局、Tween 与鼠标交互分部。
/// </summary>
/// <remarks>
///     这一部分只关心「怎么摆、怎么动、怎么响应鼠标」，集合增删和数据绑定留在主文件里。
/// </remarks>
public partial class HandCardsUI
{
    #region Tunables

    [Export] public float ArcRadius { get; set; } = 720f;

    [Export] public float MaxAngleStepDegrees { get; set; } = 7f;

    [Export] public float MaxTotalArcDegrees { get; set; } = 34f;

    [Export] public float BottomPadding { get; set; } = 16f;

    [Export] public float SidePadding { get; set; } = 24f;

    [Export] public float HoverLift { get; set; } = 26f;

    [Export] public float HoverScale { get; set; } = 1.08f;

    [Export] public float EntryDrop { get; set; } = 40f;

    [Export] public float LayoutMoveDuration { get; set; } = 0.18f;

    [Export] public float LiftDuration { get; set; } = 0.12f;

    [Export] public float ScaleDuration { get; set; } = 0.12f;

    private const float EntryStartScale = 0.9f;
    private const float MinStep = 0.0001f;

    #endregion


    #region Presentation State

    private sealed class CardState
    {
        public (Tween? X, Tween? Y, Tween? Rotation, Tween? Scale) Tweens;
        public (Action Entered, Action Exited, Action Pressed) Handlers;
        public Vector2 BasePosition;
        public float BaseRotation;
    }

    private readonly Dictionary<CardUI, CardState> _states = [];
    private readonly List<CardUI> _orderBuffer = [];

    private bool _layoutValid;
    private CardUI? _hovered;
    private CardUI? _dragging;
    private CardUI? _pointerCard;
    private Vector2 _dragOffset;

    #endregion


    #region Layout

    private void InitializeLayout()
    {
        Resized += Relayout;
        Relayout();
        // 锚点尺寸有时要在 _Ready 之后才落定，补一次。
        Callable.From(Relayout).CallDeferred();
    }

    private void AttachCard(CardUI card)
    {
        Action entered = () => OnCardMouseEntered(card);
        Action exited = () => OnCardMouseExited(card);
        Action pressed = () => OnCardLeftPressed(card);

        card.MouseEntered += entered;
        card.MouseExited  += exited;
        card.LeftPressed  += pressed;

        _states[card] = new CardState
        {
            Handlers = (entered, exited, pressed),
        };
    }

    private void DetachCardState(CardUI card)
    {
        var state = _states[card];

        card.MouseEntered -= state.Handlers.Entered;
        card.MouseExited  -= state.Handlers.Exited;
        card.LeftPressed  -= state.Handlers.Pressed;

        KillTween(state.Tweens.X);
        KillTween(state.Tweens.Y);
        KillTween(state.Tweens.Rotation);
        KillTween(state.Tweens.Scale);
        state.Tweens = default;

        if (_hovered == card) _hovered         = null;
        if (_dragging == card) _dragging       = null;
        if (_pointerCard == card) _pointerCard = null;

        _states.Remove(card);
    }

    private void ResetInteraction()
    {
        _hovered     = null;
        _dragging    = null;
        _pointerCard = null;
        _dragOffset  = default;
    }

    /// <summary>
    ///     按逻辑顺序排列子节点；若某张牌正在悬停/拖动，则把它以及它两侧的牌按
    ///     「前段正序、后段倒序、焦点牌最后」的顺序摆放，保证重叠时所有牌都能看见。
    /// </summary>
    private void ApplyChildOrder()
    {
        var focused = _dragging ?? _hovered;
        var focusIndex = focused is null ? -1 : _cards.IndexOf(focused);
        if (focusIndex < 0)
        {
            for (var i = 0; i < _cards.Count; i++) MoveChild(_cards[i], i);
            return;
        }

        _orderBuffer.Clear();
        for (var i = 0; i < focusIndex; i++) _orderBuffer.Add(_cards[i]);
        for (var i = _cards.Count - 1; i > focusIndex; i--) _orderBuffer.Add(_cards[i]);
        _orderBuffer.Add(_cards[focusIndex]);

        for (var i = 0; i < _orderBuffer.Count; i++) MoveChild(_orderBuffer[i], i);
    }

    private void Relayout() => Relayout(null);

    /// <param name="entering">刚插入的牌：本次只更新基准，不启动横向/纵向 Tween，交给调用方做入场动画。</param>
    /// <returns>是否完成了一次有效布局。</returns>
    private bool Relayout(CardUI? entering)
    {
        if (_cards.Count == 0) return false;
        if (Size.X <= 0 || Size.Y <= 0)
        {
            _layoutValid = false;
            return false;
        }

        var cardSize = _cards[0].Size;
        if (cardSize == Vector2.Zero) cardSize = _cards[0].CustomMinimumSize;
        if (cardSize == Vector2.Zero) return false;

        var count = _cards.Count;
        var step = ComputeStep(count, cardSize);
        var maxTilt = step * (count - 1) * 0.5f;
        var maxDrop = ArcRadius * (1f - Mathf.Cos(maxTilt));
        var projectedHalfHeight = cardSize.Y * 0.5f * Mathf.Cos(maxTilt)
          + cardSize.X * 0.5f * Mathf.Sin(maxTilt);
        var baseCenterY = Size.Y - BottomPadding - maxDrop - projectedHalfHeight;

        var firstLayout = !_layoutValid;
        _layoutValid = true;

        for (var i = 0; i < count; i++)
        {
            var card = _cards[i];
            var state = _states[card];
            var t = i - (count - 1) * 0.5f;
            var angle = t * step;
            var center = new Vector2(
                Size.X * 0.5f + ArcRadius * Mathf.Sin(angle),
                baseCenterY + ArcRadius * (1f - Mathf.Cos(angle))
            );

            state.BasePosition = center - cardSize * 0.5f;
            state.BaseRotation = angle;
            card.PivotOffset   = cardSize * 0.5f;

            if (firstLayout)
            {
                KillTween(state.Tweens.X);
                KillTween(state.Tweens.Y);
                KillTween(state.Tweens.Rotation);
                KillTween(state.Tweens.Scale);
                state.Tweens = default;

                card.Position = card == _hovered
                    ? state.BasePosition - new Vector2(0, HoverLift)
                    : state.BasePosition;
                card.Rotation = state.BaseRotation;
                card.Scale    = card == _hovered ? Vector2.One * HoverScale : Vector2.One;
                continue;
            }

            if (card == _dragging || card == entering) continue;

            TweenX(card, state.BasePosition.X, LayoutMoveDuration);
            TweenY(card, TargetY(card), LayoutMoveDuration);
            TweenRotation(card, state.BaseRotation, LayoutMoveDuration);
        }

        return true;
    }

    private float ComputeStep(int count, Vector2 cardSize)
    {
        if (count <= 1) return 0;

        var maxTotalArc = Mathf.DegToRad(MaxTotalArcDegrees);
        var step = Mathf.Min(Mathf.DegToRad(MaxAngleStepDegrees), maxTotalArc / (count - 1));
        var halfWidth = Size.X * 0.5f - SidePadding;

        while (step > MinStep)
        {
            var maxTilt = step * (count - 1) * 0.5f;
            var halfSpan = ArcRadius * Mathf.Sin(maxTilt)
              + cardSize.X * 0.5f * Mathf.Cos(maxTilt)
              + cardSize.Y * 0.5f * Mathf.Sin(maxTilt);
            if (halfSpan <= halfWidth) break;
            step *= 0.95f;
        }

        return step;
    }

    private void StartEntryAnimation(CardUI card)
    {
        var state = _states[card];
        card.Position = new Vector2(state.BasePosition.X, state.BasePosition.Y + EntryDrop);
        card.Rotation = state.BaseRotation;
        card.Scale    = Vector2.One * EntryStartScale;

        TweenY(card, TargetY(card), LayoutMoveDuration);
        TweenScale(card, TargetScale(card), ScaleDuration);
    }

    private float TargetY(CardUI card) =>
        _states[card].BasePosition.Y - (card == _hovered ? HoverLift : 0f);

    private float TargetScale(CardUI card) => card == _hovered ? HoverScale : 1f;

    #endregion


    #region Tween

    private static void KillTween(Tween? tween)
    {
        if (tween is not null && GodotObject.IsInstanceValid(tween)) tween.Kill();
    }

    private Tween NewTween()
    {
        var tween = CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        return tween;
    }

    private void TweenX(CardUI card, float target, double duration)
    {
        if (!IsInsideTree())
        {
            card.Position = new Vector2(target, card.Position.Y);
            return;
        }

        var state = _states[card];
        KillTween(state.Tweens.X);

        var tween = NewTween();
        tween.TweenProperty(card, "position:x", target, duration);
        state.Tweens.X = tween;
    }

    private void TweenY(CardUI card, float target, double duration)
    {
        if (!IsInsideTree())
        {
            card.Position = new Vector2(card.Position.X, target);
            return;
        }

        var state = _states[card];
        KillTween(state.Tweens.Y);

        var tween = NewTween();
        tween.TweenProperty(card, "position:y", target, duration);
        state.Tweens.Y = tween;
    }

    private void TweenScale(CardUI card, float target, double duration)
    {
        var scale = new Vector2(target, target);
        if (!IsInsideTree())
        {
            card.Scale = scale;
            return;
        }

        var state = _states[card];
        KillTween(state.Tweens.Scale);

        var tween = NewTween();
        tween.TweenProperty(card, "scale", scale, duration);
        state.Tweens.Scale = tween;
    }

    private void TweenRotation(CardUI card, float target, double duration)
    {
        if (!IsInsideTree())
        {
            card.Rotation = target;
            return;
        }

        var state = _states[card];
        KillTween(state.Tweens.Rotation);

        var tween = NewTween();
        tween.TweenProperty(card, "rotation", target, duration);
        state.Tweens.Rotation = tween;
    }

    #endregion


    #region Hover & Drag

    private void OnCardMouseEntered(CardUI card)
    {
        _pointerCard = card;
        if (_dragging is not null) return;
        SetHovered(card);
    }

    private void OnCardMouseExited(CardUI card)
    {
        if (_pointerCard == card) _pointerCard = null;
        if (_dragging is not null) return;
        if (_hovered == card) SetHovered(null);
    }

    private void SetHovered(CardUI? card)
    {
        if (_hovered == card) return;

        var old = _hovered;
        _hovered = card;
        ApplyChildOrder();

        if (old is not null && old != card)
        {
            TweenY(old, TargetY(old), LiftDuration);
            TweenScale(old, TargetScale(old), ScaleDuration);
        }

        if (card is not null)
        {
            TweenY(card, TargetY(card), LiftDuration);
            TweenScale(card, TargetScale(card), ScaleDuration);
        }
    }

    private void OnCardLeftPressed(CardUI card)
    {
        if (_dragging is not null) return;

        _pointerCard = card;
        _hovered     = card;
        _dragging    = card;
        ApplyChildOrder();

        var state = _states[card];
        KillTween(state.Tweens.X);
        KillTween(state.Tweens.Y);
        KillTween(state.Tweens.Rotation);
        KillTween(state.Tweens.Scale);
        state.Tweens = default;

        TweenScale(card, HoverScale, ScaleDuration);
        _dragOffset = GetLocalMousePosition() - card.Position;
    }

    private bool PrepareReplacement(CardUI card)
    {
        if (_dragging != card) return false;

        _dragging = null;
        _hovered  = null;
        return true;
    }

    private void FinishReplacement(CardUI card)
    {
        if (_pointerCard == card) SetHovered(card);
        else TweenScale(card, 1f, ScaleDuration);
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsVisibleInTree() || _dragging is not { } card) return;

        switch (@event)
        {
            case InputEventMouseMotion:
                card.Position = GetLocalMousePosition() - _dragOffset;
                break;
            case InputEventMouseButton { ButtonIndex: MouseButton.Left, Pressed: false }:
                EndDrag();
                GetViewport().SetInputAsHandled();
                break;
        }
    }

    private void EndDrag()
    {
        if (_dragging is not { } card) return;
        _dragging = null;

        // 拖动中牌一直跟着鼠标，_pointerCard 很可能仍是它；但牌接下来要回位，
        // 不能因此让它保持悬停。只有当指针真的落在另一张牌上时才交接悬停。
        var hovered = _pointerCard == card ? null : _pointerCard;
        _pointerCard = hovered;
        _hovered     = null;

        if (hovered is not null && _cards.Contains(hovered)) SetHovered(hovered);
        else ApplyChildOrder();

        TweenX(card, _states[card].BasePosition.X, LayoutMoveDuration);
        TweenY(card, TargetY(card), LiftDuration);
        TweenRotation(card, _states[card].BaseRotation, LayoutMoveDuration);
        TweenScale(card, TargetScale(card), ScaleDuration);
    }

    private void CancelDrag()
    {
        if (_dragging is not { } card) return;

        _dragging    = null;
        _hovered     = null;
        _pointerCard = null;
        ApplyChildOrder();

        TweenX(card, _states[card].BasePosition.X, LayoutMoveDuration);
        TweenY(card, TargetY(card), LiftDuration);
        TweenRotation(card, _states[card].BaseRotation, LayoutMoveDuration);
        TweenScale(card, TargetScale(card), ScaleDuration);
    }

    public override void _Notification(int what)
    {
        if (what != (int)NotificationWMWindowFocusOut &&
            what != (int)NotificationApplicationFocusOut) return;

        CancelDrag();
    }

    #endregion
}
