using Godot;

#if TOOLS

namespace BabelRush.Exportation;

[Tool]
public partial class Plugin : EditorPlugin
{
    private ExportPlugin? _exportPlugin;

    public override void _EnterTree()
    {
        // Initialization of the plugin goes here.
        AddExportPlugin(_exportPlugin = new ExportPlugin());
        GD.Print("Exportation plugin loaded.");
    }

    public override void _ExitTree()
    {
        // Clean-up of the plugin goes here.
        RemoveExportPlugin(_exportPlugin);
        GD.Print("Exportation plugin unloaded.");
    }
}

#endif