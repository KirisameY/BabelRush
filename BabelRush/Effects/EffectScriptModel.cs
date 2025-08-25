using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;

using KirisameLib.Extensions;

using NLua;
using NLua.Exceptions;

namespace BabelRush.Effects;

internal record EffectScriptModel(string Id, LuaFunction Action) : IScriptModel<EffectScript>
{
    public (RegKey, EffectScript) Convert(string nameSpace, string path)
    {
        RegKey id = (nameSpace, Id);
        var effect = new EffectScript(id, Action);
        return (id, effect);
    }

    public static IReadOnlyCollection<IModel<EffectScript>> FromSource(ScriptSourceInfo source, out ModelParseErrorInfo errorMessages)
    {
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

        if (returnValues is not [LuaFunction func, ..])
        {
            var msg = $"Invalid script return values: [{returnValues.Select(o => o.GetType().Name).Join(", ")}] "
              + $"(expected [LuaFunction]).";
            errorMessages = new ModelParseErrorInfo(1, [msg]);
            return [];
        }

        errorMessages = ModelParseErrorInfo.Empty;
        return [new EffectScriptModel(source.Path.Join('/'), func)];
    }
}