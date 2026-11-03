using System.Collections.Generic;
using System.IO;

using Tomlyn;

namespace BabelRushR.Game.Assets;

// ReSharper disable once InconsistentNaming
public static class L10nAssetRedirectionLoader
{
    public static void LoadFromTomlStream(string locale, Stream stream)
    {
        var dict = TomlSerializer.Deserialize<Dictionary<string, string>>(stream)!;
        foreach (var (from, to) in dict)
        {
            AssetLoader.AddL10nRedirect(locale, from, to);
        }
    }
}