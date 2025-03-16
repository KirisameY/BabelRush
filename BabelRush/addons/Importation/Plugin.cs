using Godot;

#if TOOLS

namespace BabelRush.Importation;

[Tool]
public partial class Plugin : EditorPlugin
{
    private LuaImportPlugin? _luaImportPlugin;

    public override void _EnterTree()
    {
        // Initialization of the plugin goes here.
        AddImportPlugin(_luaImportPlugin = new());
        GD.Print("Importation plugin loaded.");
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        RemoveImportPlugin(_luaImportPlugin);
        GD.Print("Importation plugin unloaded.");
    }
}

#endif