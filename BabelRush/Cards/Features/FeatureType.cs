using BabelRush.Data;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Cards.Features;

public class FeatureType(RegKey id, RegKey iconId)
{
    public RegKey Id { get; } = id;
    public NameDesc NameDesc => CardFeatureRegisters.FeatureNameDesc[Id];
    public Texture2D Icon => CardFeatureRegisters.FeatureIcon[iconId];

    public Feature NewInstance()
    {
        var result = new Feature(this);
        return result;
    }

    public static FeatureType Default { get; } = new("default:default", "default:default");
}