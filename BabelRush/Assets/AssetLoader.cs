using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

using Godot;

using KirisameY.Registration.Data;

namespace BabelRushR.Game.Assets;

public static class AssetLoader
{
    private static readonly ConcurrentDictionary<string, Dictionary<string, string>> _redirects = [];

    [return: NotNullIfNotNull(nameof(fallback))]
    public static T? Load<T>(string path, RegKey key, string extension, T? fallback = null) where T : Resource
    {
        var fullPath = $"res://@assets/{key.NameSpace}/{path}/{key.Key}.{extension}";

        if (
            _redirects.TryGetValue(TranslationServer.GetLocale(), out var d) &&
            d.TryGetValue(fullPath, out var redirected)
        ) fullPath = redirected;

        if (!ResourceLoader.Exists(fullPath)) return fallback;
        return ResourceLoader.Load(fullPath) as T ?? fallback;
    }

    // ReSharper disable once InconsistentNaming
    public static void SetL10nRedirect(string locale, string source, string target) =>
        _redirects.GetOrAdd(locale, _ => [])[source] = target;
}