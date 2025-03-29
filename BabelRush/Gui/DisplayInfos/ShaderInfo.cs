using System;
using System.Text;
using System.Text.RegularExpressions;

using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registers;

using Godot;

using KirisameLib.Event;

namespace BabelRush.Gui.DisplayInfos;

// todo: waiting for https://github.com/godotengine/godot/pull/90436 to make additional texture support

public partial class ShaderInfo
{
    #region Initialize

    [GeneratedRegex(@"^\s*#include\s*<\s*([A-Za-z_]\w*:?[A-Za-z_]\w*)\s*>\s*$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex IncludeRegex { get; }

    public ShaderInfo(string nameSpace, string shaderCode)
    {
        RegisterEventSource.LocalRegisterDone.RegisterDone += () =>
        {
            var shader = new Shader();
            var code = "#define _LOADED_\n" + IncludeRegex.Replace(shaderCode, IncludeEvaluator(nameSpace));
            shader.SetCode(code);
            _shader = shader;
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


    private Shader? _shader;

    public Shader Shader => _shader ?? throw new InvalidOperationException("Shader is not initialized yet.");
}