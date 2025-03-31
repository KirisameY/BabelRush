using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

using BabelRush.Data;
using BabelRush.I18n;
using BabelRush.Registers;

using Godot;

using JetBrains.Annotations;

using Tomlyn;

namespace BabelRush.Gui.DisplayInfos;

public class SpriteInfo(RegKey textureId, RegKey shaderInstanceId, FrozenDictionary<StringName, Variant> overrideInstanceUniforms)
{
    private readonly DynamicI18nItem<Texture2D> _texture = DynamicI18nItem.Create(SpriteInfoRegisters.Textures, textureId);
    public Texture2D Texture => _texture.Get();

    private readonly DynamicI18nItem<ShaderInstance>? _shaderInstance = DynamicI18nItem.Create(SpriteInfoRegisters.ShaderInstances, shaderInstanceId);
    public ShaderMaterial? Material => _shaderInstance?.Get().Material;

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


    public static SpriteInfo Default { get; } = new(RegKey.Default, RegKey.Default, FrozenDictionary<StringName, Variant>.Empty);

    // ReSharper disable once InconsistentNaming
    private static readonly Dictionary<RegKey, SpriteInfo> _fallbackCcache = new();
    public static Func<RegKey, SpriteInfo> Fallback => id =>
    {
        if (_fallbackCcache.TryGetValue(id, out var spriteInfo)) return spriteInfo;
        var result = _fallbackCcache[id] = new SpriteInfo(id, id, FrozenDictionary<StringName, Variant>.Empty);
        return result;
    };
}

public class SpriteInfoModel : IResModel<SpriteInfo>
{
    [IgnoreDataMember]
    public string Id { get; set; } = "";

    [UsedImplicitly]
    public string? Texture { get; set; }
    [UsedImplicitly]
    public string? Shader { get; set; }

    [UsedImplicitly]
    public Dictionary<string, string> InstanceUniform { get; set; } = new();

    public (RegKey, SpriteInfo) Convert(string nameSpace, string path)
    {
        RegKey fid = (nameSpace, Id);
        var textureId = (Texture ?? Id).WithDefaultNameSpace(nameSpace);
        var shaderId = (Shader ?? Id).WithDefaultNameSpace(nameSpace);
        var instanceUniforms = InstanceUniform.ToFrozenDictionary(
            p => new StringName(p.Key),
            p =>
            {
                Expression expression = new();
                if (expression.Parse(p.Value) is not Error.Ok) return new Variant();
                var result = expression.Execute();
                return expression.HasExecuteFailed() ? new Variant() : result;
            });

        var result = new SpriteInfo(textureId, shaderId, instanceUniforms);
        return (fid, result);
    }

    public static IReadOnlyCollection<IModel<SpriteInfo>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        if (!source.Files.TryGetValue(".toml", out var toml))
        {
            errorMessages = new(1, [".toml file not found"]);
            return [];
        }

        Toml.Parse(toml).TryToModel<SpriteInfoModel>(out var model, out var diagnostics);
        errorMessages = new(diagnostics.Count, diagnostics.Select(msg => msg.ToString()).ToArray());
        if (model is null) return [];
        model.Id = source.Path;
        return [model];
    }
}