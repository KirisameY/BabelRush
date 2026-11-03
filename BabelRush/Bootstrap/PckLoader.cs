using System;
using System.IO;
using System.Threading.Tasks;

using BabelRushR.Core.Contents;

using Godot;

using Zio;

using FileAccess = System.IO.FileAccess;
using GDFileAccess = Godot.FileAccess;

namespace BabelRushR.Game.Bootstrap;

[AutoContentsLoader("/assets.pck")]
public sealed class PckLoader : IContentsLoader<PckLoader>, IDisposable
{
    // FUCK GODOT
    private readonly MemoryStream _cache = new();

    public void Load(string packageName, FileSystemEntry entry)
    {
        if (entry is not FileEntry file) return;
        using (var stream = file.Open(FileMode.Open, FileAccess.Read))
        {
            _cache.SetLength(0);
            stream.CopyTo(_cache);
        }

        // FUCK GODOT for I cannot load pck from memory
        using var tmpFile = GDFileAccess.CreateTemp(GDFileAccess.ModeFlags.WriteRead, "br_assets", "pck");
        tmpFile.StoreBuffer(_cache.GetBuffer());
        tmpFile.Flush();
        tmpFile.Close();
        // FUCK GODOT
        ProjectSettings.LoadResourcePack(tmpFile.GetPath());
    }

    public static PckLoader Create() => new();

    // FUCK GODOT
    public void Dispose()
    {
        _cache.Dispose();
    }
}