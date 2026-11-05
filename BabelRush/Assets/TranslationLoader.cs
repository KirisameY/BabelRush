using System;
using System.Collections.Generic;
using System.IO;

using BabelRushR.Core.Common;

using Godot;

using Microsoft.Extensions.Logging;

using Tomlyn;
using Tomlyn.Model;

using Zio;

using ZLogger;

namespace BabelRushR.Game.Assets;

public static class TranslationLoader
{
    private static ILogger Logger => field ??= Logging.Logger(typeof(TranslationLoader));

    public static void LoadFromTomlStream(string locale, string packName, UPath filePath, Stream stream)
    {
        TomlTable table;
        try
        {
            table = TomlSerializer.Deserialize<TomlTable>(stream)!;
        }
        catch (TomlException e)
        {
            Logger.ZLogError($"Error while loading translation in pack {packName}, file {filePath}: {e.Message}");
            return;
        }
        if (table.TryGetValue("$namespace", out var ns) && ns is string nst) packName = nst;
        var flattenDict = FlattenFromTomlTable(table);

        var translation = new Translation();
        translation.SetLocale(locale);
        foreach (var (key, value) in flattenDict)
        {
            translation.AddMessage($"{packName}:{key}", value);
        }
        TranslationServer.AddTranslation(translation);
    }

    private static Dictionary<string, string> FlattenFromTomlTable(TomlTable fromTable)
    {
        Dictionary<string, string> result = [];
        Stack<(string Prefix, TomlTable Table)> stack = [];
        stack.Push(("", fromTable));

        while (stack.TryPop(out var t))
        {
            var (prefix, table) = t;
            foreach (var (key, value) in table)
            {
                var k = prefix is "" ? key : $"{prefix}.{key}";
                if (value is string v) result.Add(k, v);
                if (value is TomlTable subTable) stack.Push((k, subTable));
            }
        }

        return result;
    }
}