using System.Linq;

using BabelRush.Data;

using KirisameLib.Extensions;
using KirisameLib.Logging;

using NLua;
using NLua.Exceptions;

namespace BabelRush.Effects;

public class EffectScript(RegKey id, LuaFunction? luaFunc)
{
    internal EffectScriptInstance CreateInstance()
    {
        const string logProcess = nameof(CreateInstance);

        if (luaFunc is null)
        {
            Logger.Log(LogLevel.Warning, logProcess, $"Default empty script used in {id}");
            return new(id, null);
        }

        LuaTable? table = null;
        try
        {
            var result = luaFunc.Call();
            if (result is [LuaTable luaTable]) table = luaTable;
            else
            {
                var types = result.Select(r => r.GetType().Name).Join(", ");
                Logger.Log(LogLevel.Error, logProcess, $"Unexpected result of instance create function: [{types}] from {id}"
                         + $" (expected [LuaTable])");
            }
        }
        catch (LuaScriptException e)
        {
            Logger.Log(LogLevel.Error, logProcess, $"Script exception thrown from {id}: {e.Message}");
        }

        return new(id, table);
    }


    public static EffectScript Default { get; } = new(RegKey.Default, null);


    private static Logger Logger { get; } = Game.LogBus.GetLogger("EffectScript");
}

internal class EffectScriptInstance(RegKey id, LuaTable? table)
{
    private const string LogProcessCallLua = "CallLua";


    internal void Applied(Effect instance)
    {
        const string funcName = "applied";

        var func = GetFunc(funcName);
        if (func is null) return;

        try { func.Call(instance); }
        catch (LuaScriptException e)
        {
            Logger.Log(LogLevel.Error, LogProcessCallLua, $"Script exception thrown from func '{funcName}' of instance of {id}: {e.Message}");
        }
    }

    internal void Process(Effect instance, double delta)
    {
        const string funcName = "process";

        var func = GetFunc(funcName);
        if (func is null) return;

        try { func.Call(instance, delta); }
        catch (LuaScriptException e)
        {
            Logger.Log(LogLevel.Error, LogProcessCallLua, $"Script exception thrown from func '{funcName}' of instance of {id}: {e.Message}");
        }
    }

    internal bool BeforeRemoved(Effect instance)
    {
        const string funcName = "before_removed";

        var func = GetFunc(funcName);
        if (func is null) return true;

        bool result = true;
        try
        {
            var res = func.Call(instance);
            if (res is [bool resBool, ..]) result = resBool;
            else
            {
                var types = res.Select(r => r.GetType().Name).Join(", ");
                Logger.Log(LogLevel.Warning, LogProcessCallLua, $"Unexpected '{funcName}' function result: [{types}]"
                         + $" from instance of {id} (expected [bool])");
            }
        }
        catch (LuaScriptException e)
        {
            Logger.Log(LogLevel.Error, LogProcessCallLua, $"Script exception thrown from func '{funcName}' of instance of {id}: {e.Message}");
        }

        return result;
    }

    internal bool BeforeValueUpdated(Effect instance, ref int newValue)
    {
        const string funcName = "before_value_updated";

        var func = GetFunc(funcName);
        if (func is null) return true;

        bool result = true;
        try
        {
            var res = func.Call(instance, newValue);
            (bool succeed, result, newValue) = res switch
            {
                [bool b, double d, ..] => (true, b, (int)d),
                [bool b, ..]           => (true, b, newValue),
                _                      => (false, true, newValue)
            };
            if (!succeed)
            {
                var types = res.Select(r => r.GetType().Name).Join(", ");
                Logger.Log(LogLevel.Warning, LogProcessCallLua, $"Unexpected '{funcName}' function result: [{types}]"
                         + $" from instance of {id} (expected [bool, (number)])");
            }
        }
        catch (LuaScriptException e)
        {
            Logger.Log(LogLevel.Error, LogProcessCallLua, $"Script exception thrown from func '{funcName}' of instance of {id}: {e.Message}");
        }

        return result;
    }

    internal bool BeforeTimeUpdated(Effect instance, ref double newTime)
    {
        const string funcName = "before_time_updated";

        var func = GetFunc(funcName);
        if (func is null) return true;

        bool result = true;
        try
        {
            var res = func.Call(instance, newTime);
            (bool succeed, result, newTime) = res switch
            {
                [bool b, double d, ..] => (true, b, d),
                [bool b, ..]           => (true, b, newTime),
                _                      => (false, true, newTime)
            };
            if (!succeed)
            {
                var types = res.Select(r => r.GetType().Name).Join(", ");
                Logger.Log(LogLevel.Warning, LogProcessCallLua, $"Unexpected '{funcName}' function result: [{types}]"
                         + $" from instance of {id} (expected [bool, (number)])");
            }
        }
        catch (LuaScriptException e)
        {
            Logger.Log(LogLevel.Error, LogProcessCallLua, $"Script exception thrown from func '{funcName}' of instance of {id}: {e.Message}");
        }

        return result;
    }


    private LuaFunction? GetFunc(string funcName)
    {
        var funcObj = table?[funcName];
        if (funcObj is null)
        {
            return null;
        }
        if (funcObj is not LuaFunction func)
        {
            Logger.Log(LogLevel.Warning, LogProcessCallLua, $"'{funcName}' of effect instance of {id} is {funcObj.GetType()} "
                     + $"(expected LuaFunction)");
            return null;
        }
        return func;
    }


    private static Logger Logger { get; } = Game.LogBus.GetLogger("EffectScriptInstance");
}