using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Linq;

using BabelRush.Data;
using BabelRush.Registering.I18n;
using BabelRush.Registering.RootLoaders;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Extensions;

namespace BabelRush.Registering.FileLoading;

internal class LocalFileLoader(string local)
    : FileLoader
{
    // ↓这里的代码可能有毒，不过能跑所以就这样吧。

    #region Map

    private static readonly Dictionary<string, GetSourceTakers<ResSourceInfo>> LocalResInfo = new();
    private static readonly Dictionary<string, GetSourceTakers<IDictionary<string, object>>> LocalLangInfo = new();

    private static FrozenDictionary<string, FrozenDictionary<string, RootLoader>> InitRootMap(string local)
    {
        Dictionary<string, IEnumerable<(string Local, RootLoader RootLoader)>> dict = new()
        {
            ["res"] = NewLocalRootLoader(local,  LocalResInfo,  (l, dict) => new ResRootLoader((l, dict))),
            ["lang"] = NewLocalRootLoader(local, LocalLangInfo, (l, dict) => new LangRootLoader(l, dict)),
        };

        IEnumerable<(string Root, string Local, RootLoader RootLoader)> loaders = dict.SelectMany(p =>
        {
            return p.Value.Select(t => (p.Key, t.Local, t.RootLoader));
        });

        var dicts =
            from t in loaders
            group t by t.Local into localGroup
            select KeyValuePair.Create(localGroup.Key, localGroup.ToFrozenDictionary(t => t.Root, t => t.RootLoader));

        return dicts.ToFrozenDictionary();
    }

    private delegate RootLoader CreateRootLoader<TSource>(string local, IDictionary<string, ISourceTaker<TSource>> sourceTakers);

    private static IEnumerable<(string Local, RootLoader RootLoader)> NewLocalRootLoader<TSource>
        (string local, IDictionary<string, GetSourceTakers<TSource>> getters, Func<string, IDictionary<string, ISourceTaker<TSource>>, RootLoader> loaderCreator)
    {
        var sourceTakers = getters.SelectMany(p =>
        {
            return p.Value.Invoke(local).Select(t => (t.Local, p.Key, t.SourceTaker));
        });

        return from t in sourceTakers
               let lang = t.Local
               let path = t.Key
               let sourceTaker = t.SourceTaker
               group (path, sourceTaker) by lang
               into grouped
               select (grouped.Key, loaderCreator(grouped.Key, grouped.ToDictionary(x => x.path, x => x.sourceTaker)));
    }

    private FrozenDictionary<string, FrozenDictionary<string, RootLoader>> RootMap { get; } = InitRootMap(local);

    #endregion

    protected override bool EnterRootDirectory(LinkedList<string> directoryLink, out RootLoader? rootLoader)
    {
        rootLoader = null;
        var directory = directoryLink.ToArray();
        if (directory is [] or ["local"]) return true;
        if (directory is not ["local", var local, .. var path]) return false;
        if (!RootMap.TryGetValue(local, out var dict)) return false;

        rootLoader = dict.GetOrDefault(path.Join('/'));
        return true;
    }
}