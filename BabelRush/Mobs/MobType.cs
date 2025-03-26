using BabelRush.Data;
using BabelRush.Mobs.Actions;
using BabelRush.Mobs.Animation;
using BabelRush.Registers;

namespace BabelRush.Mobs;

public class MobType(RegKey id, RegKey animationSetId, bool blocksMovement, int health, MobActionStrategy actionStrategy)
{
    public RegKey Id => id;
    public NameDesc NameDesc => MobRegisters.MobNameDesc[Id];
    public MobAnimationSet AnimationSet => MobRegisters.MobAnimationSets[animationSetId];
    public bool BlocksMovement => blocksMovement;
    public int Health => health;
    public MobActionStrategy ActionStrategy => actionStrategy;


    public Mob GetInstance(Alignment alignment)
    {
        return new Mob(this, alignment);
    }


    public static MobType Default { get; } = new("default", "default", true, 50, MobActionStrategy.Default);
}