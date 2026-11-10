using BabelRushR.Core.Card;
using BabelRushR.Core.EntityAction;

namespace BabelRushR.Core.Entity.Damage;

public abstract record DamageSource();

public abstract record EntityDamageSource(IEntity Attacker) : DamageSource;

public record EntityActionDamageSource(IEntity Attacker, IEntityAction Action) : DamageSource;

public record CardEntityActionDamageSource(IEntity Attacker, IEntityAction Action, ICard Card) : DamageSource;