using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.FileLoading;
using BabelRush.Registering.I18n;
using BabelRush.Registering.RootLoaders;

using KirisameLib.Data.Registering;

using Tomlyn.Syntax;

namespace BabelRush.Registering;

public static class MakeRegistrant
{
    public static IRegistrant<TItem> ForData<TItem, TModel>(string path) where TModel : IModel<DocumentSyntax, TItem> =>
        DataRootLoader.NewRegistrant<TItem, TModel>(path);

    public static IRegistrant<TItem> ForCommonRes<TItem, TModel>(string path) where TModel : IModel<ResSourceInfo, TItem> =>
        ResRootLoader.NewRegistrant<TItem, TModel>(path);

    public static I18nRegistrant<TItem> ForLocalRes<TItem, TModel>(string path) where TModel : IModel<ResSourceInfo, TItem> =>
        LocalFileLoader.GetResRegistrant<TItem, TModel>(path);

    public static I18nRegistrant<TItem> ForLang<TItem, TModel>(string path) where TModel : IModel<IDictionary<string, object>, TItem> =>
        LocalFileLoader.GetLangRegistrant<TItem, TModel>(path);
}