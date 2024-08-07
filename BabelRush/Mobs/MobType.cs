namespace BabelRush.Mobs;

public class MobType
{
    public Mob GetInstance()
    {
        return new Mob(this);
    }

    public static MobType Default { get; } = new();
}