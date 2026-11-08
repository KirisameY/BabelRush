using BabelRushR.Core.Entity.Health;

namespace BabelRushR.Core.Entity.Damage;

public readonly record struct EntityDamage(DamageSource Source, int Value);

public readonly record struct EntityDamageResult(EntityHealthSnapshot Before, EntityHealthSnapshot After);