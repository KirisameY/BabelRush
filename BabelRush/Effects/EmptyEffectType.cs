using BabelRush.Data;

namespace BabelRush.Effects;

public class EmptyEffectType(RegKey id, RegKey iconId, EffectPolarity polarity) : EffectType(id, iconId, polarity);