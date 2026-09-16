using KirisameY.EventBus;

namespace BabelRushR.Core.Entity;

/// <summary>
/// 实体相关事件的基类型。事件总线按基类分发，订阅它即可一次收到全部实体事件。
/// </summary>
public abstract record EntityEvent(IEntity Entity) : BaseEvent;

/// <summary>实体已被加入场景之后发布。</summary>
public record EntityAddedEvent(IEntity Entity) : EntityEvent(Entity);

/// <summary>实体已脱离场景（无论是被显式移除还是死亡清扫）。</summary>
public record EntityRemovedEvent(IEntity Entity) : EntityEvent(Entity);

/// <summary>
/// 实体因 HP 归零被场景清扫时发布。死亡必然意味着已脱离场景，因此派生自
/// <see cref="EntityRemovedEvent"/>：只 post 这一个事件，基类分发会同时送达
/// <see cref="EntityRemovedEvent"/> 与 <see cref="EntityEvent"/> 的订阅者。
/// </summary>
public record EntityDiedEvent(IEntity Entity) : EntityRemovedEvent(Entity);
