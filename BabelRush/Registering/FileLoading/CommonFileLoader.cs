using System;
using System.Collections.Generic;

using BabelRush.Registering.RootLoaders;

using KirisameLib.Extensions;

namespace BabelRush.Registering.FileLoading;

internal class CommonFileLoader : FileLoader
{
    private static Dictionary<string, Func<RootLoader>> RootMap { get; } = new()
    {
        ["data"] = static () => new DataRootLoader(),
        // ["script"] = null, todo: script也该安排了
        ["res"] = static () => new ResRootLoader(),
    };


    protected override bool EnterRootDirectory(LinkedList<string> directoryLink, out RootLoader? rootLoader)
    {
        rootLoader = null;
        if (directoryLink.First is { Value: "local" }) return false;
        rootLoader = RootMap.GetOrDefault(directoryLink.Join('/'))?.Invoke();
        return true;
    }
}