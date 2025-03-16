using System;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Data;

using Godot;

using NLua;

namespace BabelRush.Scripting;

public static class ScriptHub
{
    public static void Initialize()
    {
        Lua = new Lua();
        var initialization = ResourceLoader.Load<Text>("res://Scripting/initialize.lua").Content;

        Lua.LoadCLRPackage();
        Lua.DoString(initialization);
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