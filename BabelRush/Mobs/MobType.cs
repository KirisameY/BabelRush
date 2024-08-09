namespace BabelRush.Mobs;

public class MobType(bool blocksMovement)
{
    public bool BlocksMovement { get; } = blocksMovement;


    public Mob GetInstance(Alignment alignment)
    {
        return new Mob(this, alignment);
    }


    public static MobType Default { get; } = new(true);
}