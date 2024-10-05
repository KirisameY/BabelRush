using BabelRush.Misc;
using BabelRush.Mobs.Animation;
using BabelRush.Registers;

namespace BabelRush.Mobs;

public class MobType(string id, MobAnimationSet animationSet, bool blocksMovement)
{
    public string Id { get; } = id;
    public NameDesc NameDesc => MobRegisters.MobNameDesc.GetItem(Id);
    public MobAnimationSet AnimationSet { get; } = animationSet;
    public bool BlocksMovement { get; } = blocksMovement;


    public Mob GetInstance(Alignment alignment)
    {
        return new Mob(this, alignment);
    }


    public static MobType Default { get; } = new("default", MobAnimationSet.Default, true);
}