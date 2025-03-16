using BabelRush.Scripting;

using Godot;

namespace BabelRush.Tests.Scripting;

public partial class LuaInteractionTest : Node
{
    public override void _Ready()
    {
        var lua = ScriptHub.Lua;
        var test1 = lua.DoString(
            """
            t1 = 't1'
            local t2 = 't2'
            return t1, t2
            """
        );
        var test2 = lua.DoString(
            """
            return t1, t2
            """
        );
        GD.Print(test1);
        GD.Print(test2);

        var test3 = lua.DoString(
            """
            local e = luanet.import_type("BabelRush.GamePlay.ApChangedEvent")
            GD.Print(e(12,24))
            return e(11,2)
            """
        );
        GD.Print(test3[0].GetType());
        GD.Print(test3[0]);
        GD.Print(test3);


        var func1 = lua.LoadString(
            """
            local a = 1
            GD.Print("this is func1")
            GD.Print("a=", a)
            """,
            "func"
        );
        var func2 = lua.LoadString(
            """
            GD.Print("this is func2")
            GD.Print("a=", a)
            """,
            "func"
        );
        var func3 = lua.LoadString(
            """
            local a = 3
            GD.Print("this is func3")
            GD.Print("a=", a)
            """,
            "func"
        );

        func1.Call();
        func2.Call();
        func3.Call();
        func2.Call();
        func1.Call();
    }
}