using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Data;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Gui.DisplayInfos;

public class ShaderInstance(RegKey shaderId, IEnumerable<KeyValuePair<StringName, Variant>> uniforms, IEnumerable<KeyValuePair<StringName, Variant>> instanceUniforms)
{
    private readonly DynamicI18nItem<ShaderMaterial> _material =
        DynamicI18nItem.Create<RegKey, ShaderInfo, ShaderMaterial>(SpriteInfoRegisters.ShaderInfos, shaderId, (pre, s) =>
        {
            var result = pre ?? new ShaderMaterial { Shader = s.Shader };
            result.Shader = s.Shader;
            return result;
        });

    public ShaderMaterial Material => _material.Get();

    [field: AllowNull, MaybeNull]
    public FrozenDictionary<StringName, Variant> Uniforms => field ??= uniforms.ToFrozenDictionary();

    [field: AllowNull, MaybeNull]
    public FrozenDictionary<StringName, Variant> DefaultInstanceUniforms => field ??= instanceUniforms.ToFrozenDictionary();
}