using BabelRush.Data;

using Godot;
using Godot.Collections;

#if TOOLS

namespace BabelRush.Importation;

public partial class LuaImportPlugin : EditorImportPlugin
{
    public override string _GetImporterName() => "BabelRush.Lua";

    public override string _GetVisibleName() => "Lua Script";

    public override string[] _GetRecognizedExtensions() => ["lua"];

    public override string _GetSaveExtension() => "tres";

    public override string _GetResourceType() => "Text";

    public override int _GetPresetCount() => 0;

    public override string _GetPresetName(int presetIndex) => "";

    public override float _GetPriority() => 1;

    public override int _GetImportOrder() => 0;

    public override bool _CanImportThreaded() => false;

    public override int _GetFormatVersion() => 1;

    public override Array<Dictionary> _GetImportOptions(string path, int presetIndex) => [];

    public override bool _GetOptionVisibility(string path, StringName optionName, Dictionary options) => true;

    public override Error _Import(string sourceFile, string savePath, Dictionary options,
                                  Array<string> platformVariants, Array<string> genFiles)
    {
        using var file = FileAccess.Open(sourceFile, FileAccess.ModeFlags.Read);
        var error = file.GetError();
        if (error != Error.Ok) return error;

        var text = new Text { Content = file.GetAsText() };
        string filename = $"{savePath}.{_GetSaveExtension()}";
        return ResourceSaver.Save(text, filename);
    }
}

#endif