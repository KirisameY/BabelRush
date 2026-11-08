using BabelRushR.Core.Numeric;

using KirisameY.EventBus.EventComponents;
using KirisameY.Numeric;

namespace BabelRushR.Core.Entity.Damage;

public abstract record EntityDamageEvent(EntityDamage Damage, IEntity Entity) : EntityEvent(Entity);

public sealed record EntityDamageRequestEvent(
    EntityDamage Damage, IEntity Entity, EventCancelToken CancelToken
) : EntityDamageEvent(Damage, Entity), ICancelableEvent;

public sealed record EntityDamageBeforeEvent(
    EntityDamage Damage, IEntity Entity,
    IModifierEditableNumeric<int, CommonNumericModifyOrder> Value
) : EntityDamageEvent(Damage, Entity);

public sealed record EntityDamageAfterEvent(
    EntityDamage Damage, IEntity Entity,
    int Value, EntityDamageResult Result
) : EntityDamageEvent(Damage, Entity);