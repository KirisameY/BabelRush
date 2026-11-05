using System.IO;
using System.Linq;

using BabelRushR.Core.Contents;

using KirisameY.Relinq.Extensions;

using Zio;

namespace BabelRushR.Game.Assets;

[AutoContentsLoader("/local")]
// ReSharper disable once InconsistentNaming
public class I18nDataLoader : IContentsLoader<I18nDataLoader>
{
    public void Load(string packageName, FileSystemEntry entry)
    {
        if (entry is not DirectoryEntry directory) return;
        var fs = directory.FileSystem;
        foreach (var localeDir in directory.EnumerateDirectories())
        {
            var path = localeDir.Path;
            var locale = localeDir.Name;
            if (fs.TryGetFileSystemEntry(path / "translations") is DirectoryEntry trDir)
            {
                trDir.EnumerateFiles("*.toml", SearchOption.AllDirectories)
                     .Select(f => (path: f.Path, str: f.Open(FileMode.Open, FileAccess.Read)))
                     .SelectSelf(t => TranslationLoader.LoadFromTomlStream(locale, packageName, t.path, t.str))
                     .ForEach(t => t.str.Dispose());
            }
            if (fs.TryGetFileSystemEntry(path / "redirections") is DirectoryEntry reDir)
            {
                reDir.EnumerateFiles("*.toml", SearchOption.AllDirectories)
                     .Select(f => (path: f.Path, str: f.Open(FileMode.Open, FileAccess.Read)))
                     .SelectSelf(t => L10nAssetRedirectionLoader.LoadFromTomlStream(locale, packageName, t.path, t.str))
                     .ForEach(t => t.str.Dispose());
            }
        }
    }

    public static I18nDataLoader Create() => new();
}