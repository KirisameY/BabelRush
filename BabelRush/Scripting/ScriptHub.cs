using System;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Data;

using Godot;

using NLua;

namespace BabelRush.Scripting;

public class ScriptHub
{
    internal ScriptHub()
    {
        Lua = new Lua();
        var initialization = ResourceLoader.Load<Text>("res://Scripting/initialize.lua").Content;
        var sandboxLoader = ResourceLoader.Load<Text>("res://Scripting/sandbox_loader.lua").Content;

        Lua.LoadCLRPackage();
        Env           = (LuaTable)Lua.DoString(initialization)[0];
        SandboxLoader = (LuaFunction)Lua.DoString(sandboxLoader)[0];
    }

    [field: AllowNull, MaybeNull]
    private Lua Lua { get; }

    private LuaFunction SandboxLoader { get; }
    private LuaTable Env { get; }


    // Methods
    private object[] LoadStringCode(object code, LuaTable? modEnv) => SandboxLoader.Call(code, Env, modEnv);

    private bool TryLoadStringCode(object code, LuaTable? modEnv,
                                   [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
    {
        (bool result, function, err) = LoadStringCode(code, modEnv) switch
        {
            [LuaFunction f]    => (true, f, (string?)null),
            [null, string msg] => (false, null, msg),
            _                  => throw new Exception("Idk why the fuck result of parsing a lua script is neither [func] nor [nil, err]")
        };

        return result;
    }

    public bool TryLoadString(string code, [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
        => TryLoadStringCode(code, null, out function, out err);

    public bool TryLoadString(byte[] code, [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
        => TryLoadStringCode(code, null, out function, out err);

    public bool TryLoadString(string code, LuaTable? modEnv,
                              [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
        => TryLoadStringCode(code, modEnv, out function, out err);

    public bool TryLoadString(byte[] code, LuaTable? modEnv,
                              [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
        => TryLoadStringCode(code, modEnv, out function, out err);

    public LuaFunction LoadString(string code, LuaTable? modEnv = null) => (LuaFunction)SandboxLoader.Call(code, Env, modEnv)[0];
    public LuaFunction LoadString(byte[] code, LuaTable? modEnv = null) => (LuaFunction)SandboxLoader.Call(code, Env, modEnv)[0];
}