namespace BabelRush.Mobs;

public class MobType
{
    public Mob GetInstance(Alignment alignment)
    {
        return new Mob(this, alignment);
    }


    public static MobType Default { get; } = new();
}