using BabelRush.Data;
using BabelRush.Gui.DisplayInfos;
using BabelRush.Registers;

namespace BabelRush.Effects;

public class EffectType(RegKey id, RegKey iconId)
{
    public RegKey Id => id;
    public NameDesc NameDesc => EffectRegisters.EffectNameDesc[id];
    public SpriteInfo Icon => EffectRegisters.EffectIcon[iconId];


    public static EffectType Default { get; } = new(RegKey.Default, RegKey.Default);
}