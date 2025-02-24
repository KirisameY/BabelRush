namespace BabelRush.Mobs.Actions;

/// <summary>
/// You can also call it <c> MobActionStrategizerType </c>.
/// </summary>
public abstract class MobActionStrategy
{
    public abstract MobActionStrategizer NewInstance(Mob mob);

    public static MobActionStrategy Default => EmptyMobActionStrategy.Instance;
}