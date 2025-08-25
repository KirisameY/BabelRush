using BabelRush.Data;
using BabelRush.Registers;

namespace BabelRush.Effects;

public class ScriptEffectType(RegKey id, RegKey iconId, EffectPolarity polarity, RegKey scriptId) : EffectType(id, iconId, polarity)
{
    private EffectScript Script => EffectRegisters.EffectScripts[scriptId];

    public override Effect CreateInstance(int value) => new ScriptEffect(this, value, Script.CreateInstance());
}

file class ScriptEffect(EffectType type, int value, EffectScriptInstance script) : Effect(type, value)
{
    protected override void Applied() => script.Applied(this);


    protected override void Process(double delta) => script.Process(this, delta);

    protected override bool BeforeRemoved() => script.BeforeRemoved(this);

    protected override bool BeforeValueUpdated(ref int newValue) => script.BeforeValueUpdated(this, ref newValue);

    protected override bool BeforeTimeUpdated(ref double newTime) => script.BeforeTimeUpdated(this, ref newTime);
}