using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Mobs;

using KirisameLib.Logging;

using NLua;
using NLua.Exceptions;

namespace BabelRush.Actions;

public class ScriptActionStep(RegKey id, LuaFunction action) : ActionStep
{
    public override void Act(Mob self, IReadOnlyList<Mob> targets, int value)
    {
        try { action.Call(self, targets, value); }
        catch (LuaScriptException e)
        {
            Logger.Log(LogLevel.Error, nameof(Act), $"Script exception thrown from {id}: {e.Message}");
        }
    }


    // Logging
    private static readonly Logger Logger = Game.LogBus.GetLogger(nameof(ScriptActionStep));
}