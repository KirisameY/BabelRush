using System.Diagnostics.CodeAnalysis;

namespace BabelRush.Mobs.Actions;

public sealed class EmptyMobActionStrategy : MobActionStrategy
{
    private EmptyMobActionStrategy() { }

    [field: AllowNull, MaybeNull]
    public static EmptyMobActionStrategy Instance => field ??= new();

    public override MobActionStrategizer NewInstance(Mob mob) => new EmptyMobActionStrategizer(mob);
}

public sealed class EmptyMobActionStrategizer(Mob mob) : MobActionStrategizer(mob, EmptyMobActionStrategy.Instance)
{
    public override MobAction? GetNextAction() => null;
}