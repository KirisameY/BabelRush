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
///     圆弧不再逐张算 x/y：卡片都是圆心节点 <see cref="CardCenter"/> 的子节点，
///     自身旋转给出卡片在圆上的角度，offset 变换的 y 位给出半径，两者合起来就是卡片的位置。
///     卡片是复用的（手牌只是它的容器之一），所以 pivot / offset 这些「当手牌」才有的约定
///     也由本类设置与还原（见 <see cref="EnterHandLayout"/> / <see cref="ExitHandLayout"/>），<see cref="CardUI"/> 不认识手牌区。
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
        public (Tween? Position, Tween? Rotation, Tween? Radius, Tween? Scale) Tweens;
        public (Action Entered, Action Exited, Action Pressed, Action Released) Handlers;
        public float BaseRotation;
    }

    private readonly Dictionary<CardUI, CardState> _states = [];
    private readonly List<CardUI> _orderBuffer = [];

    /// <summary>圆弧圆心：尺寸为 0，只用来当卡片的父节点；旋转它等于让整把牌绕圆心转。</summary>
    private Control CardCenter => field ??= GetNode<Control>("CardCenter");

    /// <summary>打出判定线：牌心高过这条线就算拖出手牌区。挂在手牌区下面，随窗口一起动。</summary>
    private Marker2D PlayThreshold => field ??= GetNode<Marker2D>("PlayThreshold");

    private bool _layoutValid;
    private float _step;
    private Vector2 _centerBase;
    private Tween? _centerTween;
    private CardUI? _hovered;
    private CardUI? _dragging;
    private CardUI? _returning; // 松手后正在飞回原位的那张；落位前指针事件不作数
    private CardUI? _pointerCard;
    private Vector2 _dragOffset;

    #endregion


    #region Hand Layout

    /// <summary>
    ///     卡片进入手牌区时的几何约定：自身底部居中落在父节点原点，旋转中心移到卡片底边中点。
    /// </summary>
    /// <remarks>
    ///     圆心节点在屏幕下方，所以「在圆弧的哪个角度」直接用 <see cref="Control.Rotation"/> 表达；
    ///     半径由 offset 变换的 y 位补上——offset 变换套在节点变换里面，会被旋转一起带走，
    ///     于是「转个角度 + 沿 -y 推一个半径」正好把卡片摆到圆弧上。
    ///     必须同时关掉 <see cref="Control.OffsetTransformVisualOnly"/>，否则鼠标命中会留在没被推走的位置。
    ///     这一组属性只对「当手牌」有意义，所以由本类负责设置和还原，<see cref="CardUI"/> 不认识手牌区。
    /// </remarks>
    private static void EnterHandLayout(CardUI card)
    {
        card.PivotOffset      = Vector2.Zero;
        card.PivotOffsetRatio = new Vector2(0.5f, 1f);
        card.Position         = HandBasePosition(card);
        card.Rotation         = 0f;
        card.Scale            = Vector2.One;

        card.OffsetTransformPivot      = Vector2.Zero;
        card.OffsetTransformPivotRatio = new Vector2(0.5f, 1f);
        card.OffsetTransformPosition   = Vector2.Zero;
        card.OffsetTransformRotation   = 0f;
        card.OffsetTransformScale      = Vector2.One;
        card.OffsetTransformVisualOnly = false;
        card.OffsetTransformEnabled    = true;
    }

    /// <summary>离开手牌区时把旋转中心与 offset 变换恢复成默认值，让卡片回到普通 Control 的语义。</summary>
    private static void ExitHandLayout(CardUI card)
    {
        card.OffsetTransformEnabled    = false;
        card.OffsetTransformVisualOnly = true;
        card.OffsetTransformPosition   = Vector2.Zero;
        card.OffsetTransformRotation   = 0f;
        card.OffsetTransformScale      = Vector2.One;
        card.OffsetTransformPivot      = Vector2.Zero;
        card.OffsetTransformPivotRatio = new Vector2(0.5f, 0.5f);

        card.PivotOffset      = Vector2.Zero;
        card.PivotOffsetRatio = Vector2.Zero;
        card.Position         = Vector2.Zero;
        card.Rotation         = 0f;
        card.Scale            = Vector2.One;
    }

    /// <summary>底部居中落在圆心原点时的布局位置；入场动画与拖拽回位都以它为准。</summary>
    private static Vector2 HandBasePosition(CardUI card)
    {
        var size = card.Size == Vector2.Zero ? card.CustomMinimumSize : card.Size;
        return new Vector2(-size.X * 0.5f, -size.Y);
    }

    /// <summary>把卡片放到离圆心 <paramref name="radius"/> 远的圆弧上。</summary>
    private static void SetHandRadius(CardUI card, float radius) =>
        card.OffsetTransformPosition = new Vector2(0f, RadialOffset(radius));

    /// <summary>「离圆心多远」在 offset 变换的 y 上是负值：圆心在牌的下方。</summary>
    private static float RadialOffset(float radius) => -radius;

    #endregion


    #region Layout

    /// <summary>
    ///     圆心的基准位置：圆心在牌的下方一个半径处，高度要让最外侧那张牌的最低点（旋转后的外侧下角）
    ///     正好贴着 <see cref="BottomPadding"/>，也就是把 <paramref name="maxTilt"/> 造成的下坠抬回来。
    /// </summary>
    private Vector2 CenterBaseOf(float maxTilt, Vector2 cardSize) => new(
        Size.X * 0.5f,
        Size.Y - BottomPadding + ArcRadius * Mathf.Cos(maxTilt) - cardSize.X * 0.5f * Mathf.Sin(maxTilt)
    );

    private void InitializeLayout()
    {
        Resized += Relayout;
        Relayout();
        // 锚点尺寸有时要在 _Ready 之后才落定，补一次。
        Callable.From(Relayout).CallDeferred();
    }

    private void AttachCard(CardUI card)
    {
        Action entered = () => OnCardMouseEntered(card),
               exited = () => OnCardMouseExited(card),
               pressed = () => OnCardPressed(card),
               released = () => OnCardReleased(card);

        card.MouseEntered += entered;
        card.MouseExited  += exited;
        card.ButtonDown   += pressed;
        card.ButtonUp     += released;

        _states[card] = new CardState
        {
            Handlers = (entered, exited, pressed, released),
        };
    }

    private void DetachCardState(CardUI card)
    {
        if (!_states.Remove(card, out var state)) return;

        card.MouseEntered -= state.Handlers.Entered;
        card.MouseExited  -= state.Handlers.Exited;
        card.ButtonDown   -= state.Handlers.Pressed;
        card.ButtonUp     -= state.Handlers.Released;

        if (_hovered == card) _hovered         = null;
        if (_dragging == card) _dragging       = null;
        if (_returning == card) _returning     = null;
        if (_pointerCard == card) _pointerCard = null;
    }

    private void ResetInteraction()
    {
        _hovered     = null;
        _dragging    = null;
        _returning   = null;
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
            for (var i = 0; i < _cards.Count; i++) CardCenter.MoveChild(_cards[i], i);
            return;
        }

        _orderBuffer.Clear();
        for (var i = 0; i < focusIndex; i++) _orderBuffer.Add(_cards[i]);
        for (var i = _cards.Count - 1; i > focusIndex; i--) _orderBuffer.Add(_cards[i]);
        _orderBuffer.Add(_cards[focusIndex]);

        for (var i = 0; i < _orderBuffer.Count; i++) CardCenter.MoveChild(_orderBuffer[i], i);
    }

    /// <summary>第 <paramref name="index"/> 张牌在圆弧上的角度；0 是正中间（圆弧最高点），正角度靠左。</summary>
    private float AngleAt(int index) => (index - (_cards.Count - 1) * 0.5f) * _step;

    /// <summary>卡片离圆心的距离；悬停的牌被推远一点，于是沿半径方向抬起来。</summary>
    private float RadiusFor(CardUI card) => ArcRadius + (card == _hovered ? HoverLift : 0f);

    private float TargetScale(CardUI card) => card == _hovered ? HoverScale : 1f;

    private void Relayout() => Relayout(null);

    /// <param name="entering">刚插入的牌：本次只更新基准，交给调用方做入场动画。</param>
    /// <returns>是否完成了一次有效布局。</returns>
    private bool Relayout(CardUI? entering)
    {
        if (_cards.Count == 0)
        {
            ResetCardCenter();
            return false;
        }

        if (Size.X <= 0 || Size.Y <= 0)
        {
            _layoutValid = false;
            return false;
        }

        var cardSize = _cards[0].Size;
        if (cardSize == Vector2.Zero) cardSize = _cards[0].CustomMinimumSize;
        if (cardSize == Vector2.Zero) return false;

        _step       = ComputeStep(_cards.Count, cardSize);
        _centerBase = CenterBaseOf(_step * (_cards.Count - 1) * 0.5f, cardSize);
        var firstLayout = !_layoutValid;
        _layoutValid = true;

        for (var i = 0; i < _cards.Count; i++)
        {
            var card = _cards[i];
            var state = _states[card];
            var angle = AngleAt(i);
            state.BaseRotation = angle;

            if (firstLayout)
            {
                KillTweens(state);

                card.Position = HandBasePosition(card);
                card.Rotation = angle;
                SetHandRadius(card, RadiusFor(card));
                card.OffsetTransformScale = Vector2.One * TargetScale(card);
                continue;
            }

            if (card == _dragging || card == entering) continue;

            TweenRotation(card, angle, LayoutMoveDuration);
            TweenRadius(card, RadiusFor(card), LayoutMoveDuration);
            TweenOffsetScale(card, TargetScale(card), ScaleDuration);
        }

        // 圆心的 y 直接落位（窗口缩放时跟手），横向与旋转交给 Tween——只有选中牌时它们才会变。
        CardCenter.Position = new Vector2(CardCenter.Position.X, _centerBase.Y);
        TweenCardCenter(firstLayout);

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

        // 入场只做下沉与放大；角度与半径先落位——Relayout 对新加入的牌会跳过它们。
        card.Rotation = state.BaseRotation;
        SetHandRadius(card, RadiusFor(card));
        card.OffsetTransformScale = Vector2.One * EntryStartScale;
        card.Position             = HandBasePosition(card) + new Vector2(0f, EntryDrop);

        TweenPosition(card, HandBasePosition(card), LayoutMoveDuration);
        TweenOffsetScale(card, TargetScale(card), ScaleDuration);
    }

    /// <summary>
    ///     悬停（选中）的牌在圆弧上的角度与半径；没有选中牌时都是 0。
    /// </summary>
    private (float Angle, float Radius) FocusTarget()
    {
        if (_hovered is not { } card || !_states.TryGetValue(card, out var state)) return (0f, 0f);
        return (state.BaseRotation, RadiusFor(card));
    }

    /// <summary>
    ///     让整把牌绕圆心转到「悬停的牌朝正上方」的姿态，同时把圆心横向挪到这张牌原来的位置，
    ///     于是选中牌的横向位置不变，其余牌跟着转同样的角度、仍落在同一段圆弧上。
    /// </summary>
    private void TweenCardCenter(bool immediate = false)
    {
        var (angle, radius) = FocusTarget();
        var target = new Vector2(_centerBase.X + radius * Mathf.Sin(angle), _centerBase.Y);

        if (immediate || !IsInsideTree())
        {
            _centerTween?.Kill();
            _centerTween = null;

            CardCenter.Position = target;
            CardCenter.Rotation = -angle;
            return;
        }

        _centerTween?.Kill();

        var tween = NewTween(CardCenter);
        tween.SetParallel();
        tween.TweenProperty(CardCenter, "rotation", -angle, LayoutMoveDuration);
        tween.TweenProperty(CardCenter, "position:x", target.X, LayoutMoveDuration);
        _centerTween = tween;
    }

    private void ResetCardCenter()
    {
        _centerTween?.Kill();
        _centerTween = null;

        _centerBase         = CenterBaseOf(0f, Vector2.Zero);
        CardCenter.Rotation = 0f;
        CardCenter.Position = _centerBase;
    }

    #endregion


    #region Tween

    private Tween NewTween(Node target) // 从节点创建，让 tween 的生命周期跟着节点走，不用额外清理。
    {
        var tween = target.CreateTween();
        tween.SetTrans(Tween.TransitionType.Cubic).SetEase(Tween.EaseType.Out);
        return tween;
    }

    private static void KillTweens(CardState state)
    {
        state.Tweens.Position?.Kill();
        state.Tweens.Rotation?.Kill();
        state.Tweens.Radius?.Kill();
        state.Tweens.Scale?.Kill();
        state.Tweens = default;
    }

    private Tween? TweenPosition(CardUI card, Vector2 target, double duration)
    {
        if (!IsInsideTree())
        {
            card.Position = target;
            return null;
        }

        var state = _states[card];
        state.Tweens.Position?.Kill();

        var tween = NewTween(card);
        tween.TweenProperty(card, "position", target, duration);
        state.Tweens.Position = tween;
        return tween;
    }

    private void TweenRotation(CardUI card, float target, double duration)
    {
        if (!IsInsideTree())
        {
            card.Rotation = target;
            return;
        }

        var state = _states[card];
        state.Tweens.Rotation?.Kill();

        var tween = NewTween(card);
        tween.TweenProperty(card, "rotation", target, duration);
        state.Tweens.Rotation = tween;
    }

    private void TweenRadius(CardUI card, float target, double duration)
    {
        if (!IsInsideTree())
        {
            SetHandRadius(card, target);
            return;
        }

        var state = _states[card];
        state.Tweens.Radius?.Kill();

        var tween = NewTween(card);
        tween.TweenProperty(card, "offset_transform_position:y", RadialOffset(target), duration);
        state.Tweens.Radius = tween;
    }

    private void TweenOffsetScale(CardUI card, float target, double duration)
    {
        if (!IsInsideTree())
        {
            card.OffsetTransformScale = Vector2.One * target;
            return;
        }

        var state = _states[card];
        state.Tweens.Scale?.Kill();

        var tween = NewTween(card);
        tween.TweenProperty(card, "offset_transform_scale", Vector2.One * target, duration);
        state.Tweens.Scale = tween;
    }

    #endregion


    #region Hover & Drag

    private void OnCardMouseEntered(CardUI card)
    {
        _pointerCard = card;
        if (_dragging is not null || _returning is not null) return;
        SetHovered(card);
    }

    private void OnCardMouseExited(CardUI card)
    {
        if (_pointerCard == card) _pointerCard = null;
        if (_dragging is not null || _returning is not null) return;
        if (_hovered == card) SetHovered(null);
    }

    private void SetHovered(CardUI? card)
    {
        if (_hovered == card) return;

        var old = _hovered;
        _hovered = card;
        ApplyChildOrder();

        // 选中＝悬停：整把牌转到被悬停的牌朝正上方。
        if (_layoutValid) TweenCardCenter();

        if (old is not null && old != card)
        {
            TweenRadius(old, RadiusFor(old), LiftDuration);
            TweenOffsetScale(old, TargetScale(old), ScaleDuration);
        }

        if (card is not null)
        {
            TweenRadius(card, RadiusFor(card), LiftDuration);
            TweenOffsetScale(card, TargetScale(card), ScaleDuration);
        }
    }

    private void OnCardPressed(CardUI card)
    {
        if (_dragging is not null) return;

        _pointerCard = card;
        SetHovered(card); // 通常已经是它了；若不是，这一步会把圆心与抬起都转到这张牌上。
        _dragging = card;
        ApplyChildOrder();

        // 拖动只改 position，其它几个 tween 项（角度 / 半径 / 缩放 / 圆心）留在原处。
        var state = _states[card];
        state.Tweens.Position?.Kill();
        state.Tweens.Position = null;

        _dragOffset = MouseInCardCenter() - card.Position;
    }

    private void OnCardReleased(CardUI card)
    {
        if (_dragging != card) return;
        EndDrag();
    }

    private void DraggingProcess()
    {
        if (!IsVisibleInTree() || _dragging is not { } card) return;
        card.Position = MouseInCardCenter() - _dragOffset;
    }

    /// <summary>
    ///     结束拖动。<paramref name="allowPlay"/> 为 <see langword="false"/> 时只把牌放回去——
    ///     焦点丢失、数据被替换这类「不是玩家松手」的结束不该打出牌。
    /// </summary>
    private void EndDrag(bool allowPlay = true)
    {
        if (_dragging is not { } card) return;

        if (allowPlay && IsDraggedOut(card))
        {
            CardPlayRequested?.Invoke(card.ViewModel!);

            // 如果被成功打出，进行处理
            if (!_cards.Contains(card)) return; // 因为拖到了区域外所以指针现在一定没放在其它卡上，不用重新判定 hover
        }

        // 松手时指针必然还压在牌上（拖动时牌跟着指针走），看不出它底下是谁；
        // 牌飞回原位的这段时间指针事件都不作数，等落位了再一次性判定。
        _dragging  = null;
        _returning = card;
        ApplyChildOrder();

        TweenRadius(card, RadiusFor(card), LiftDuration);
        TweenOffsetScale(card, TargetScale(card), ScaleDuration);

        var tween = TweenPosition(card, HandBasePosition(card), LayoutMoveDuration);
        if (tween is null) SettleHoverAfterDrag(card);
        else tween.Finished += () => SettleHoverAfterDrag(card);
    }

    /// <summary>松手时牌心有没有越过判定线。</summary>
    /// <remarks>
    ///     判定线是场景里的 <see cref="PlayThreshold"/>（Marker2D），挂在手牌区下面，窗口缩放时跟着走；
    ///     用节点而不是一个导出数字，是为了能在编辑器里直接拖着这条线调手感。
    ///     比的是牌心而不是指针：拖动时牌和指针之间有个固定偏移，抓牌的位置不同，指针过线时牌还差得远。
    /// </remarks>
    private bool IsDraggedOut(CardUI card) => CardVisualCenter(card).Y < PlayThreshold.GlobalPosition.Y;

    /// <summary>
    ///     卡片视觉矩形的中心（全局坐标）。手牌给的旋转、半径偏移与悬停缩放都在
    ///     <see cref="Control.GetGlobalTransform"/> 里，不用再补。
    /// </summary>
    private static Vector2 CardVisualCenter(CardUI card) => card.GetGlobalTransform() * (card.Size * 0.5f);

    /// <summary>
    ///     牌落回原位后按「指针现在压着谁」重新定一次悬停。
    /// </summary>
    /// <remarks>
    ///     只测被拖动的那张牌是不够的：松手的位置通常正好落在别的牌上，那张牌才是该选中的。
    ///     指针没压着任何牌时清空悬停，让圆心转回正中的姿态。
    /// </remarks>
    private void SettleHoverAfterDrag(CardUI card)
    {
        if (_returning != card) return; // 已经被下一次拖动 / 替换接管
        _returning = null;

        if (_dragging is not null || !_cards.Contains(card) || !IsInsideTree()) return;
        SetHovered(CardUnderPointer());

        // 上面只改了本类的悬停状态，Godot 的 mouse_over 还停在被拖的那张牌上。
        // 让它按当前指针位置立刻重算一次，之后指针离开手牌区才收得到 MouseExited。
        GetViewport().UpdateMouseCursorState();
    }

    /// <summary>
    ///     指针现在真正压着的那张牌：按绘制顺序从上层往下层找，和 Godot 自己的命中顺序一致
    ///     （同一位置叠着几张时，画在上面的赢）；没压着任何牌时返回 <see langword="null"/>。
    /// </summary>
    private CardUI? CardUnderPointer()
    {
        for (var i = CardCenter.GetChildCount() - 1; i >= 0; i--)
        {
            if (CardCenter.GetChild(i) is CardUI card && _states.ContainsKey(card) && IsPointerOver(card))
                return card;
        }

        return null;
    }

    /// <summary>指针是否真的落在这张牌当前的视觉矩形上（含旋转与 offset 变换）。</summary>
    private bool IsPointerOver(CardUI card)
    {
        var local = card.GetGlobalTransform().AffineInverse() * GetGlobalMousePosition();
        return new Rect2(Vector2.Zero, card.Size).HasPoint(local);
    }

    /// <summary>替换数据前先把这张牌从拖动里摘出来，之后 <see cref="Relayout()"/> 才能正常安排它。</summary>
    /// <remarks>换数据不是玩家松手，不能顺带把牌打出去，所以走 <c>allowPlay: false</c>。</remarks>
    private bool PrepareReplacement(CardUI card)
    {
        if (_dragging != card) return false;

        EndDrag(false);
        _hovered = null;
        if (_layoutValid) TweenCardCenter();
        return true;
    }

    private void FinishReplacement(CardUI card)
    {
        if (_pointerCard == card)
        {
            SetHovered(card);
            return;
        }

        TweenRadius(card, RadiusFor(card), LiftDuration);
        TweenOffsetScale(card, TargetScale(card), ScaleDuration);
    }

    /// <summary>鼠标在圆心节点局部坐标系里的位置：卡片是圆心的子节点，position 也在那个坐标系里。</summary>
    private Vector2 MouseInCardCenter()
    {
        var local = GetLocalMousePosition() - CardCenter.Position;
        return CardCenter.Rotation == 0f ? local : local.Rotated(-CardCenter.Rotation);
    }

    public override void _Notification(int what)
    {
        // 窗口失去焦点不是玩家松手：拖动取消，牌放回原位，不能顺手打出去。
        if (what is (int)NotificationWMWindowFocusOut or (int)NotificationApplicationFocusOut)
            EndDrag(false);
    }

    #endregion
}