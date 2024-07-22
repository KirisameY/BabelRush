namespace BabelRush.Mobs;

public class CommonMob : IMob
{
    public int Health { get; set; }
    public int Shield { get; set; }

    public static CommonMob Default { get; } = new();
}