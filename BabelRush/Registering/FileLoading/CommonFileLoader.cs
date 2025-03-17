using System;
using System.Collections.Generic;

using BabelRush.Registering.RootLoaders;

using KirisameLib.Extensions;

namespace BabelRush.Registering.FileLoading;

internal class CommonFileLoader : FileLoader
{
    private static Dictionary<string, Func<RootLoader>> RootMap { get; } = new()
    {
        [RootNames.Script] = static () => new ScriptRootLoader(),
        [RootNames.Data] = static () => new DataRootLoader(),
        [RootNames.Res] = static () => new ResRootLoader(),
    };


    protected override bool EnterRootDirectory(LinkedList<string> directoryLink, out RootLoader? rootLoader)
    {
        rootLoader = null;
        if (directoryLink.First is { Value: "local" }) return false;
        rootLoader = RootMap.GetOrDefault(directoryLink.Join('/'))?.Invoke();
        return true;
    }
}