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
        object[] returnValues;

        try
        {
            returnValues = source.Script.Call();
        }
        catch (LuaScriptException e)
        {
            errorMessages = new(1, [e.ToString()]);
            return [];
        }

        if (returnValues is not [LuaTable table, ..])
        {
            var msg = $"Invalid script return values: [{returnValues.Select(o => o.GetType().Name).Join(", ")}] "
              + $"(expected [LuaTable]).";
            errorMessages = new ModelParseErrorInfo(1, [msg]);
            return [];
        }

        errorMessages = ModelParseErrorInfo.Empty;
        return [new ScriptModuleModel(source.Path.Join('/'), table)];
    }
}