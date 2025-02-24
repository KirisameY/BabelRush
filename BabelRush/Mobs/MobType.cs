using BabelRush.Data;
using BabelRush.Mobs.Actions;
using BabelRush.Mobs.Animation;
using BabelRush.Registers;

namespace BabelRush.Mobs;

public class MobType(string id, MobAnimationSet animationSet, bool blocksMovement, int health, MobActionStrategy actionStrategy)
{
    public string Id => id;
    public NameDesc NameDesc => MobRegisters.MobNameDesc.GetItem(Id);
    public MobAnimationSet AnimationSet => animationSet;
    public bool BlocksMovement => blocksMovement;
    public int Health => health;
    public MobActionStrategy ActionStrategy => actionStrategy;


    public Mob GetInstance(Alignment alignment)
    {
        return new Mob(this, alignment);
    }


    public static MobType Default { get; } = new("default", MobAnimationSet.Default, true, 50, MobActionStrategy.Default);
}