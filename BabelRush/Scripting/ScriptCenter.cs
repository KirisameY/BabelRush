using Godot;

using NLua;

namespace BabelRush.Scripting;

public static class ScriptCenter
{
    static ScriptCenter()
    {
        // 可能以后要动态构建这个来加载来自资源包的程序集
        const string preExecution = """
            import('BabelRush')
            import('GodotSharp', 'Godot')
            import = function () end
            GD.Print('Lua frame loaded')
            """;

        Lua.LoadCLRPackage();
        Lua.DoString(preExecution);
    }

    public static Lua Lua { get; } = new();
}