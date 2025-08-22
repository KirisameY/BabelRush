using BabelRush.Data;

namespace BabelRush.Effects;

internal class ScriptEffectTypeModel : EffectTypeModel
{
    public string? Script { get; set; } = null;

    protected override (RegKey, EffectType) Convert(string nameSpace, RegKey id, RegKey icon, EffectPolarity polarity)
    {
        var script = Script?.WithDefaultNameSpace(nameSpace) ?? id;
        return (id, new ScriptEffectType(id, icon, polarity, script));
    }
}