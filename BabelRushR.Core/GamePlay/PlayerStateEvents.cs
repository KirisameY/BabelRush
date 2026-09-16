using KirisameY.EventBus;

namespace BabelRushR.Core.GamePlay;

/// <summary>
/// 玩家状态相关事件的基类型。事件总线按基类分发，订阅它即可一次收到全部玩家状态事件。
/// </summary>
public abstract record PlayerStateEvent : BaseEvent;

/// <summary>
/// 整数费用 AP 发生变化时发布。
/// <see cref="IPlayerState.APRegenerated"/> 的小数累积不发事件，只做属性变化通知。
/// </summary>
public record APChangedEvent(int OldAP, int NewAP) : PlayerStateEvent;
