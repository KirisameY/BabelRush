using KirisameY.EventBus;

namespace BabelRushR.Core.Entity;

/// <summary>
/// 实体相关事件的基类型。
/// </summary>
public abstract record EntityEvent(IEntity Entity) : BaseEvent;

/// <summary>实体已被加入场景之后发布。</summary>
public record EntityAddedEvent(IEntity Entity) : EntityEvent(Entity);

/// <summary>实体已因任何原因脱离场景。</summary>
public record EntityRemovedEvent(IEntity Entity) : EntityEvent(Entity);

/// <summary>
/// 实体因 HP 归零被场景清扫时发布，派生自 <see cref="EntityRemovedEvent"/>。
/// </summary>
public record EntityDiedEvent(IEntity Entity) : EntityRemovedEvent(Entity);
