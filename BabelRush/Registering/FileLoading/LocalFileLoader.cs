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

internal class LocalFileLoader(string nameSpace, bool overwriting, string local) : FileLoader, IRootLoader
{
    // ↓这里的代码可能有毒，不过能跑所以就这样吧。

    #region Map

    private static readonly Dictionary<string, II18nSourceTakerFactory<ResSourceInfo>> LocalResInfo = new();
    private static readonly Dictionary<string, II18nSourceTakerFactory<IDictionary<string, object>>> LocalLangInfo = new();

    private static FrozenDictionary<string, FrozenDictionary<string, IRootLoader>> InitRootMap(string nameSpace, bool overwriting, string local)
    {
        Dictionary<string, IEnumerable<(string Local, IRootLoader RootLoader)>> dict = new()
        {
            [RootNames.Res] = NewLocalRootLoader(local, LocalResInfo, (l, dict) =>
                                                     new ResRootLoader(nameSpace, overwriting, (l, dict))),
            [RootNames.Lang] = NewLocalRootLoader(local, LocalLangInfo, (l, dict) =>
                                                      new LangRootLoader(nameSpace, overwriting, l, dict)),
        };

        IEnumerable<(string Root, string Local, IRootLoader RootLoader)> loaders = dict.SelectMany(p =>
        {
            return p.Value.Select(t => (p.Key, t.Local, t.RootLoader));
        });

        var dicts =
            from t in loaders
            group t by t.Local into localGroup
            select KeyValuePair.Create(localGroup.Key, localGroup.ToFrozenDictionary(t => t.Root, t => t.RootLoader));

        return dicts.ToFrozenDictionary();
    }

    private static IEnumerable<(string Local, IRootLoader RootLoader)> NewLocalRootLoader<TSource>(
        string local, IDictionary<string, II18nSourceTakerFactory<TSource>> getters,
        Func<string, IDictionary<string, SourceTakerRegistrant<TSource>>, IRootLoader> loaderCreator)
    {
        var sourceTakers = getters.SelectMany(p =>
        {
            return p.Value.CreateSourceTakers(local).Select(t => (t.Local, p.Key, t.SourceTaker));
        });

        return from t in sourceTakers
               let lang = t.Local
               let path = t.Key
               let sourceTaker = t.SourceTaker
               group (path, sourceTaker) by lang
               into grouped
               select (grouped.Key, loaderCreator(grouped.Key, grouped.ToDictionary(x => x.path, x => x.sourceTaker)));
    }

    private FrozenDictionary<string, FrozenDictionary<string, IRootLoader>> RootMap { get; } = InitRootMap(nameSpace, overwriting, local);

    #endregion


    // Registrant Getter
    public static T WithResSourceTakerFactory<T>(string path, T newFac) where T : II18nSourceTakerFactory<ResSourceInfo>
    {
        LocalResInfo.Add(path, newFac);
        return newFac;
    }

    public static T WithLangSourceTakerFactory<T>(string path, T newFac) where T : II18nSourceTakerFactory<IDictionary<string, object>>
    {
        LocalLangInfo.Add(path, newFac);
        return newFac;
    }


    protected override bool EnterRootDirectory(LinkedList<string> directoryLink, out IRootLoader? rootLoader)
    {
        rootLoader = null;
        var directory = directoryLink.ToArray();
        if (directory is [] or ["local"] or ["overwriting"]) return true; // skip common dir
        if (!overwriting && directory is ["overwriting", var ns])         // overwriting root loader
        {
            rootLoader = new LocalFileLoader(ns, true, local);
            return true;
        }

        if (directory is not ["local", var pLocal, .. var path]) return false;
        if (!RootMap.TryGetValue(pLocal, out var dict)) return false;

        rootLoader = dict.GetOrDefault(path.Join('/'));
        return true;
    }
}