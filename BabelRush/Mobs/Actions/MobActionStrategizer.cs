namespace BabelRush.Mobs.Actions;

public abstract class MobActionStrategizer(Mob mob, MobActionStrategy strategy) //todo: 记得给别的东西也abstract掉
{
    public Mob Mob => mob;
    public MobActionStrategy Strategy => strategy;

    public abstract MobAction? GetNextAction();
}