using BabelRush.Misc;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Cards.Features;

public class FeatureType(string id)
{
    public string Id { get; } = id;
    public NameDesc NameDesc => CardFeatureRegisters.FeatureNameDesc.GetItem(Id);
    public Texture2D Icon => CardFeatureRegisters.FeatureIcon.GetItem(id);

    public Feature NewInstance()
    {
        var result = new Feature(this);
        return result;
    }

    public static FeatureType Default { get; } = new("default");
}