using BabelRush.Scripting;

using Godot;

namespace BabelRush.Tests.Scripting;

public partial class LuaInteractionTest : Node
{
    public override void _Ready()
    {
        var lua = ScriptCenter.Lua;
        var test1 = lua.DoString(
            """
            t1 = 't1'
            local t2 = 't2'
            return t1, t2
            """);
        var test2 = lua.DoString(
            """
            return t1, t2
            """
        );
        GD.Print(test1);
        GD.Print(test2);
    }
}