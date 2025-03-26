using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using KirisameLib.Extensions;

using NLua;
using NLua.Exceptions;

namespace BabelRush.Actions;

internal record ActionStepModel(string Id, LuaFunction Action) : IScriptModel<ActionStep>
{
    public (RegKey, ActionStep) Convert(string nameSpace)
    {
        RegKey id = (nameSpace, Id);
        var step = new ScriptActionStep(id, Action);
        return (id, step);
    }

    public static IReadOnlyCollection<IModel<ActionStep>> FromSource(ScriptSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
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

        if (returnValues is not [LuaFunction func, ..])
        {
            errors.Add($"Invalid script return values :" + $"[{returnValues.Select(o => o.GetType().ToString()).Join(", ")}].");
            errorMessages = new ModelParseErrorInfo(errors.Count, errors.ToArray());
            return [];
        }

        errorMessages = new ModelParseErrorInfo(errors.Count, errors.ToArray());
        return [new ActionStepModel(source.Path.Join('/'), func)];
    }
}