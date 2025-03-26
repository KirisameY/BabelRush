using System.Collections.Generic;
using System.Linq;

using BabelRush.Registering.RootLoaders;

using KirisameLib.Extensions;

namespace BabelRush.Registering.FileLoading;

internal class CommonFileLoader(string nameSpace, bool overwriting) : FileLoader, IRootLoader
{
    private delegate IRootLoader CreateRootLoader(string nameSpace, bool overwriting);

    private static Dictionary<string, CreateRootLoader> RootMap { get; } = new()
    {
        [RootNames.Script] = static (ns, o) => new ScriptRootLoader(ns, o),
        [RootNames.Data] = static (ns, o) => new DataRootLoader(ns, o),
        [RootNames.Res] = static (ns, o) => new ResRootLoader(ns, o),
    };


    protected override bool EnterRootDirectory(LinkedList<string> directoryLink, out IRootLoader? rootLoader)
    {
        rootLoader = null;
        if (directoryLink.First is { Value: "local" }) return false;            // skip local
        if (!overwriting && directoryLink.ToArray() is ["overwriting", var ns]) // overwriting root loader
        {
            rootLoader = new CommonFileLoader(ns, true);
            return true;
        }

        rootLoader = RootMap.GetOrDefault(directoryLink.Join('/'))?.Invoke(nameSpace, overwriting);
        return true;
    }
}