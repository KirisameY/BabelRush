using System;
using System.Diagnostics.CodeAnalysis;

using NLua;

namespace BabelRush.Scripting;

public static class ScriptHub
{
    public static void Initialize()
    {
        Lua = new Lua();

        // 可能以后要动态构建这个来加载来自资源包的程序集
        const string preExecution = """
            luanet.load_assembly('BabelRush')
            luanet.load_assembly('KirisameLib.Core')
            luanet.load_assembly('KirisameLib.Logging')
            luanet.load_assembly('GodotSharp', 'Godot')

            import = nil -- function () end
            luanet.load_assembly = nil
            cstype = luanet.import_type 

            -- temp code here
            GD = cstype('Godot.GD')
            local game = cstype('BabelRush.Game')
            local log_level = cstype('KirisameLib.Logging.LogLevel')
            game.LogBus:GetLogger('Lua'):Log(log_level.Info, 'Initialize', 'Lua frame loaded')

            -- 这个可以写个工具方法
            -- GD.Print(luanet.ctype(game):IsInstanceOfType(game.Instance))
            """;

        Lua.LoadCLRPackage();
        Lua.DoString(preExecution);
    }

    [field: AllowNull, MaybeNull]
    public static Lua Lua
    {
        get
        {
            if (field is null) throw new ScriptHubNotInitializedException();
            return field;
        }
        private set;
    }

    //Exceptions
    public class ScriptHubNotInitializedException : Exception;
}