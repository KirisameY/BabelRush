using BabelRush.Data;

namespace BabelRush.Effects;

internal class EmptyEffectTypeModel : EffectTypeModel
{
    protected override (RegKey, EffectType) Convert(string nameSpace, RegKey id, RegKey icon, EffectPolarity polarity) =>
        (id, new EmptyEffectType(id, icon, polarity));
}