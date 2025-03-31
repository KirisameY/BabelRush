using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using BabelRush.Data;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

using KirisameLib.Extensions;

using Tomlyn;
using Tomlyn.Model;

namespace BabelRush.Gui.DisplayInfos;

public class ShaderInstance
{
    private static readonly ShaderMaterial DefaultMaterial = new ShaderMaterial();

    public ShaderInstance(RegKey shaderId, IEnumerable<KeyValuePair<StringName, Variant>> uniforms, IEnumerable<KeyValuePair<StringName, Variant>> instanceUniforms)
    {
        _uniforms = uniforms;
        _instanceUniforms = instanceUniforms;
        _material = DynamicI18nItem.Create<RegKey, ShaderInfo, ShaderMaterial>(SpriteInfoRegisters.ShaderInfos, shaderId, (pre, s) =>
        {
            var result = pre ?? new ShaderMaterial { Shader = s.Shader };
            if (s.Shader is null) return DefaultMaterial;
            result.Shader = s.Shader;
            Uniforms.ForEach(p => result.SetShaderParameter(p.Key, p.Value));
            return result;
        });
    }


    private readonly DynamicI18nItem<ShaderMaterial> _material;
    public ShaderMaterial Material => _material.Get();

    private readonly IEnumerable<KeyValuePair<StringName, Variant>> _uniforms;
    [field: AllowNull, MaybeNull]
    public FrozenDictionary<StringName, Variant> Uniforms => field ??= _uniforms.ToFrozenDictionary();

    private readonly IEnumerable<KeyValuePair<StringName, Variant>> _instanceUniforms;
    [field: AllowNull, MaybeNull]
    public FrozenDictionary<StringName, Variant> DefaultInstanceUniforms => field ??= _instanceUniforms.ToFrozenDictionary();


    public static ShaderInstance Default { get; } = new(RegKey.Default, [], []);

    // ReSharper disable once InconsistentNaming
    private static readonly Dictionary<RegKey, ShaderInstance> _fallbackCache = new();
    public static Func<RegKey, ShaderInstance> Fallback { get; } = id =>
    {
        if (_fallbackCache.TryGetValue(id, out var shaderInstance)) return shaderInstance;
        var result = _fallbackCache[id] = new ShaderInstance(id, FrozenDictionary<StringName, Variant>.Empty, FrozenDictionary<StringName, Variant>.Empty);
        return result;
    };
}

public class ShaderInstanceModel(string id, string? shader, IReadOnlyDictionary<StringName, Variant> uniforms,
                                 IReadOnlyDictionary<StringName, Variant> defaultInstanceUniforms)
    : IResModel<ShaderInstance>
{
    public (RegKey, ShaderInstance) Convert(string nameSpace, string path)
    {
        RegKey fid = (nameSpace, id);
        RegKey shaderId = shader?.WithDefaultNameSpace(nameSpace) ?? fid;

        var finalUniforms = uniforms.Select(TextureSelector);
        var finalInstanceUniforms = defaultInstanceUniforms.Select(TextureSelector);

        var instance = new ShaderInstance(shaderId, finalUniforms, finalInstanceUniforms);
        return (fid, instance);

        KeyValuePair<StringName, Variant> TextureSelector(KeyValuePair<StringName, Variant> p)
        {
            if (p.Value.Obj is not string s) return p;
            var tid = s.WithDefaultNameSpace(nameSpace);
            var tex = SpriteInfoRegisters.Textures[tid];
            return new KeyValuePair<StringName, Variant>(p.Key, Variant.From(tex));
        }
    }

    public static IReadOnlyCollection<IModel<ShaderInstance>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        if (!source.Files.TryGetValue(".toml", out var toml))
        {
            errorMessages = new(1, [".toml file not found"]);
            return [];
        }

        var model = Toml.Parse(toml).ToModel();
        var shaderId = model.GetOrDefault(nameof(shader).ToSnakeCase()) as string;
        var uniformsDict =
            ParseUniformDict(model.GetOrDefault(nameof(uniforms).ToSnakeCase()) as TomlTable ?? []);
        var defaultInstanceUniformsDict =
            ParseUniformDict(model.GetOrDefault(nameof(defaultInstanceUniforms).ToSnakeCase()) as TomlTable ?? []);
        var result = new ShaderInstanceModel(source.Path, shaderId, uniformsDict, defaultInstanceUniformsDict);

        errorMessages = ModelParseErrorInfo.Empty;
        return [result];

        static IReadOnlyDictionary<StringName, Variant> ParseUniformDict(IDictionary<string, object>? from)
        {
            if (from is null) return new Dictionary<StringName, Variant>();
            return from.Where(p => p.Value is string).ToDictionary(p => new StringName(p.Key), p =>
            {
                var s = (string)p.Value;
                Expression expression = new();
                var error = expression.Parse(s);
                if (error is Error.Ok) return Variant.From(s);
                var v = expression.Execute();
                return expression.HasExecuteFailed() ? Variant.From(s) : v;
            });
        }
    }
}