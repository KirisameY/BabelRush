using System.Collections.Generic;
using System.IO;

using BabelRushR.Core.Common;

using Microsoft.Extensions.Logging;

using Tomlyn;

using Zio;

using ZLogger;

namespace BabelRushR.Game.Assets;

// ReSharper disable once InconsistentNaming
public static class L10nAssetRedirectionLoader
{
    private static ILogger Logger => field ??= Logging.Logger(typeof(L10nAssetRedirectionLoader));

    public static void LoadFromTomlStream(string locale, string packName, UPath filePath, Stream stream)
    {
        try
        {
            var dict = TomlSerializer.Deserialize<Dictionary<string, string>>(stream)!;
            foreach (var (from, to) in dict)
            {
                AssetLoader.AddL10nRedirect(locale, from, to);
            }
        }
        catch (TomlException e)
        {
            Logger.ZLogError($"Error while loading translation in pack {packName}, file {filePath}: {e.Message}");
        }
    }
}