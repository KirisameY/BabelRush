using System.Collections.Frozen;
using System.Linq;

using BabelRush.Data;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Gui.DisplayInfos;

public class SpriteInfo(RegKey textureId, RegKey shaderInstanceId, FrozenDictionary<StringName, Variant> overrideInstanceUniforms)
{
    private readonly DynamicI18nItem<Texture2D> _texture = DynamicI18nItem.Create(SpriteInfoRegisters.Textures, textureId);
    public Texture2D Texture => _texture.Get();

    private readonly DynamicI18nItem<ShaderInstance> _shaderInstance = DynamicI18nItem.Create(SpriteInfoRegisters.ShaderInstances, shaderInstanceId);
    public ShaderMaterial Material => _shaderInstance.Get().Material;

    private readonly DynamicI18nItem<FrozenDictionary<StringName, Variant>> _instanceUniforms = DynamicI18nItem.Create
        <RegKey, ShaderInstance, FrozenDictionary<StringName, Variant>>
        (SpriteInfoRegisters.ShaderInstances, shaderInstanceId, (_, s) =>
        {
            return overrideInstanceUniforms
                  .Concat(s.DefaultInstanceUniforms)
                  .GroupBy(p => p.Key)
                  .Select(g => g.First())
                  .ToFrozenDictionary();
        });

    public FrozenDictionary<StringName, Variant> InstanceUniforms => _instanceUniforms.Get();
}