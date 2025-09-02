using BabelRush.Data;

namespace BabelRush.Effects;

public class EmptyEffectType(RegKey id, RegKey iconId, EffectPolarity polarity) : EffectType(id, iconId, polarity)
{
    public override Effect CreateInstance(int value = 0) => new EmptyEffect(this, value);
}

file class EmptyEffect(EmptyEffectType type, int value) : Effect(type, value)
{
    protected override void Applied() { }

    protected override void Process(double delta) { }

    protected override bool BeforeRemoved() => true;
}