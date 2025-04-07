using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registers;

using Godot;

namespace BabelRush.Gui.DisplayInfos;

// todo: waiting for https://github.com/godotengine/godot/pull/90436 to make additional texture support

public partial class ShaderInfo()
{
    #region Initialize

    [GeneratedRegex(@"^\s*#include\s*<\s*([A-Za-z_]\w*(?::[A-Za-z_]\w*)?)\s*>\s*$", RegexOptions.Multiline)]
    private static partial Regex IncludeRegex { get; }

    public ShaderInfo(string nameSpace, string shaderCode) : this()
    {
        RegisterEventSource.LocalRegisterDone.RegisterDone += () =>
        {
            var shader = new Shader();
            var code = "#define _LOADED_\n" + IncludeRegex.Replace(shaderCode, IncludeEvaluator(nameSpace));
            shader.SetCode(code);
            Shader = shader;
        };

        return;

        MatchEvaluator IncludeEvaluator(string ns) => m =>
        {
            var id = m.Groups[1].Value.WithDefaultNameSpace(ns);
            var includeInfo = SpriteInfoRegisters.ShaderIncludes[id];
            var includeCode = includeInfo.Code;
            includeCode = IncludeRegex.Replace(includeCode, IncludeEvaluator(id.NameSpace));
            return includeCode;
        };
    }

    #endregion


    public Shader? Shader { get; private set; }


    public static ShaderInfo Default { get; } = new();
}

public class ShaderInfoModel(string id, string code) : IResModel<ShaderInfo>
{
    public (RegKey, ShaderInfo) Convert(string nameSpace, string path)
    {
        RegKey fid = (nameSpace, id);
        var info = new ShaderInfo(fid.NameSpace, code);
        return (fid, info);
    }

    public static IReadOnlyCollection<IModel<ShaderInfo>> FromSource(ResSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        if (!source.Files.TryGetValue(".gdshader", out var gdshader))
        {
            errorMessages = new(1, [".gdshader file not found"]);
            return [];
        }

        var code = Encoding.UTF8.GetString(gdshader);
        errorMessages = ModelParseErrorInfo.Empty;
        return [new ShaderInfoModel(source.Path, code)];
    }
}