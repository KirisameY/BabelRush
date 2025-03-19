namespace BabelRush.Mobs.Actions;

public abstract class MobActionStrategizer(MobActionStrategy strategy, Mob mob)
{
    public Mob Mob => mob;
    public MobActionStrategy Strategy => strategy;

    public abstract MobAction? GetNextAction();
}