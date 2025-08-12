using Godot;

using NLua;

namespace BabelRush.Tests.Scripting;

public partial class LuaInteractionTest : Node
{
    public override void _Ready()
    {
        var scrHub = Game.ScriptHub;

        var env = (LuaTable)scrHub.LoadString(
            """
            local env = {
                GD = cstype("Godot.GD")
            }
            return env
            """
        ).Call()[0];

        var test1 = scrHub.LoadString(
            """
            t1 = 't1'
            local t2 = 't2'
            return t1, t2
            """, env
        ).Call();
        var test2 = scrHub.LoadString(
            """
            return t1, t2
            """, env
        ).Call();
        GD.Print(test1);
        GD.Print(test2);

        var test3 = scrHub.LoadString(
            """
            local e = cstype("BabelRush.GamePlay.ApChangedEvent")
            GD.Print(e(12,24))
            return e(11,2)
            """, env
        ).Call();
        GD.Print(test3[0].GetType());
        GD.Print(test3[0]);
        GD.Print(test3);


        var func1 = scrHub.LoadString(
            """
            local a = 1
            GD.Print("this is func1")
            GD.Print("a=", a)
            """, env
        );
        var func2 = scrHub.LoadString(
            """
            GD.Print("this is func2")
            GD.Print("a=", a)
            """, env
        );
        var func3 = scrHub.LoadString(
            """
            a = 3
            GD.Print("this is func3")
            GD.Print("a=", a)
            """, env
        );

        func1.Call();
        func2.Call();
        func3.Call();
        func2.Call();
        func1.Call();

        scrHub.LoadString(
            """
            GD.Print(BabelRush.get_module('asd','dsa'))
            """, env
        ).Call();
    }
}