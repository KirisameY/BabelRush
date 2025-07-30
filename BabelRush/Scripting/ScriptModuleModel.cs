using System;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using KirisameLib.Extensions;

using NLua;
using NLua.Exceptions;

namespace BabelRush.Scripting;

internal record ScriptModuleModel(string Id, LuaTable Table) : IScriptModel<LuaTable>
{
    public (RegKey, LuaTable) Convert(string nameSpace, string path)
    {
        RegKey id = (nameSpace, Id);
        return (id, Table);
    }

    public static IReadOnlyCollection<IModel<LuaTable>> FromSource(ScriptSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
        // OPTIMIZE: 脚本这里有很长的重复代码，回头写个util
        List<string> errors = [];
        object[] returnValues = [];

        try
        {
            returnValues = source.Script.Call();
        }
        catch (LuaScriptException e)
        {
            errors.Add(e.ToString());
        }

        if (returnValues is not [LuaTable table, ..])
        {
            errors.Add($"Invalid script return values :" + $"[{returnValues.Select(o => o.GetType().ToString()).Join(", ")}].");
            errorMessages = new ModelParseErrorInfo(errors.Count, errors.ToArray());
            return [];
        }

        errorMessages = new ModelParseErrorInfo(errors.Count, errors.ToArray());
        return [new ScriptModuleModel(source.Path.Join('/'), table)];
    }
}