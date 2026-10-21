using System.Diagnostics.CodeAnalysis;

using Godot;

using KirisameY.Registration.Data;

namespace BabelRushR.Game.Assets;

public static class AssetLoader
{
    [return: NotNullIfNotNull(nameof(fallback))]
    public static T? Load<T>(string path, RegKey key, string extension, T? fallback = null) where T : Resource
    {
        var fullPath = $"res://@assets/{key.NameSpace}/{path}/{key.Key}.{extension}";

        if (!ResourceLoader.Exists(fullPath)) return fallback;
        return ResourceLoader.Load(fullPath) as T ?? fallback;
    }
}