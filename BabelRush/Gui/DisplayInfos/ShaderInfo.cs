using System;
using System.Text;
using System.Text.RegularExpressions;

using BabelRush.Data;
using BabelRush.Registering;
using BabelRush.Registers;

using Godot;

using KirisameLib.Event;

namespace BabelRush.Gui.DisplayInfos;

public partial class ShaderInfo // todo:让它不能被local加载好了
{
    [GeneratedRegex(@"^\s*#include\s*<\s*([A-Za-z_]\w*)\s*>\s*$", RegexOptions.Multiline | RegexOptions.Compiled)]
    private static partial Regex IncludeRegex { get; }

    public ShaderInfo(string nameSpace, string shaderCode)
    {
        Game.LoadEventBus.Subscribe<CommonRegisterDoneEvent>(_ =>
        {
            var shader = new Shader();
            var code = new StringBuilder();
            code.Append("#define _LOADED_\n");
            code.Append(IncludeRegex.Replace(shaderCode, m => // todo: 另外ShaderInc也需要处理include。可能得写个IncInfo类存默认命名空间然后递归
            {
                var include = m.Groups[1].Value;
                var id = include.WithDefaultNameSpace(nameSpace);
                return SpriteInfoRegisters.ShaderIncludes[id].Code;
            }));
            shader.SetCode(code.ToString());
            _shader = shader;
        }, HandlerSubscribeFlag.OnlyOnce);
    }

    private Shader? _shader;

    public Shader Shader => _shader ?? throw new InvalidOperationException("Shader is not initialized yet.");
}