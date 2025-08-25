using BabelRush.Data;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registers;

namespace BabelRush.Effects;

public abstract class EffectType(RegKey id, RegKey iconId, EffectPolarity polarity)
{
    public RegKey Id => id;
    public NameDesc NameDesc => EffectRegisters.EffectNameDesc[id];
    public SpriteInfo Icon => EffectRegisters.EffectIcon[iconId];
    public EffectPolarity Polarity => polarity;


    public abstract Effect CreateInstance(int value = 0);


    public static EffectType Default { get; } = new EmptyEffectType(RegKey.Default, RegKey.Default, EffectPolarity.None);
}