namespace BabelRush.Mobs;

public abstract class Mob
{
    public abstract int Health { get; set; }
    public abstract int Shield { get; set; }
    
    public static CommonMob Default { get; } = new();
}