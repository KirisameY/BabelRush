using System;

using BabelRush.Data;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Gui.DisplayInfos;

public class SpriteInfo(RegKey textureId, RegKey shaderInstanceId)
{
    private readonly DynamicI18nItem<Texture2D> _texture = DynamicI18nItem.Create(SpriteInfoRegisters.Textures, textureId);
    public Texture2D Texture => _texture.Get();

    private readonly DynamicI18nItem<ShaderInstance> _shaderInstance = DynamicI18nItem.Create(SpriteInfoRegisters.ShaderInstances, shaderInstanceId);
    public ShaderMaterial Material => _shaderInstance.Get().Material;
}