using System.IO;
using System.IO.Compression;
using System.Text;

using Godot;

#if TOOLS

namespace BabelRush.Exportation;

public partial class ExportPlugin : EditorExportPlugin
{
    [Export]
    public string Name { get; set; } = "";

    public override string _GetName() => "BabelRushExporting";


    //Exporting
    private const string ResPath = "res://assets/";
    private const string ResPackPath = "assets.zip";

    private ZipArchive _resPack = null!;

    public override void _ExportBegin(string[] features, bool isDebug, string path, uint flags)
    {
        var dir = Path.GetDirectoryName(path);
        var resPackFile = new FileStream($"{dir}/{ResPackPath}", FileMode.Create);
        _resPack = new ZipArchive(resPackFile, ZipArchiveMode.Create, false, Encoding.UTF8);
    }

    public override void _ExportFile(string path, string type, string[] features)
    {
        if (path.StartsWith(ResPath)) //todo: 应该把assets直接移出godot工程目录
        {
            Skip();
            var innerPath = path.Remove(0, ResPath.Length);
            var globalPath = ProjectSettings.GlobalizePath(path);
            var entry = _resPack.CreateEntry(innerPath, CompressionLevel.NoCompression);
            using var stream = entry.Open();
            using var source = File.Open(globalPath, FileMode.Open);
            source.CopyTo(stream);
        }
    }

    public override void _ExportEnd()
    {
        _resPack.Dispose();
    }
}

#endif