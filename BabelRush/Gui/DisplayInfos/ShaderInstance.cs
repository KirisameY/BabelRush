using System;

using BabelRush.Data;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Gui.DisplayInfos;

public class ShaderInstance(RegKey shaderId)
{
    private readonly DynamicI18nItem<ShaderMaterial> _material =
        DynamicI18nItem.Create(SpriteInfoRegisters.ShaderInfos, shaderId, s => new ShaderMaterial { Shader = s.Shader });
    public ShaderMaterial Material => _material.Get();
}