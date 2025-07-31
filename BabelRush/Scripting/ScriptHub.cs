using System;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Data;
using BabelRush.Registers;

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
        var modEnvGetter = ResourceLoader.Load<Text>("res://Scripting/mod_env_getter.lua").Content;

        Lua.LoadCLRPackage();
        Lua.NewTable("BabelRush");
        var modulesGetter = (string ns, string id) => MiscRegisters.Modules.GetItem(id.WithDefaultNameSpace(ns));
        Lua.RegisterFunction("BabelRush.get_module", modulesGetter.Target, modulesGetter.Method);

        Env         = (LuaTable)Lua.DoString(initialization)[0];
        SandboxLoad = (LuaFunction)Lua.DoString(sandboxLoader)[0];
        GetModEnv   = (LuaFunction)Lua.DoString(modEnvGetter)[0];
    }

    [field: AllowNull, MaybeNull]
    private Lua Lua { get; }

    private LuaTable Env { get; }
    private LuaFunction SandboxLoad { get; }
    private LuaFunction GetModEnv { get; }


    // Methods

    #region Sandboxing

    private object[] LoadStringCode(object code, LuaTable? modEnv, string? name) => SandboxLoad.Call(code, Env, modEnv, name);

    private bool TryLoadStringCode(object code, LuaTable? modEnv, string? name,
                                   [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
    {
        (bool result, function, err) = LoadStringCode(code, modEnv, name) switch
        {
            [LuaFunction f, ..] => (true, f, (string?)null),
            [null, string msg]  => (false, null, msg),
            _                   => throw new Exception("Idk why the fuck result of parsing a lua script is neither [func, ..] nor [nil, err]")
        };

        return result;
    }

    public bool TryLoadString(string code, LuaTable? modEnv, string? name,
                              [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
        => TryLoadStringCode(code, modEnv, name, out function, out err);

    public bool TryLoadString(byte[] code, LuaTable? modEnv, string? name,
                              [NotNullWhen(true)] out LuaFunction? function, [NotNullWhen(false)] out string? err)
        => TryLoadStringCode(code, modEnv, name, out function, out err);

    public LuaFunction LoadString(string code, LuaTable? modEnv = null, string? name = null) =>
        (LuaFunction)SandboxLoad.Call(code, Env, modEnv, name)[0];

    public LuaFunction LoadString(byte[] code, LuaTable? modEnv = null, string? name = null) =>
        (LuaFunction)SandboxLoad.Call(code, Env, modEnv, name)[0];

    #endregion

    internal LuaTable GetModEnvironment(string nameSpace) => (LuaTable)GetModEnv.Call(nameSpace)[0];
}