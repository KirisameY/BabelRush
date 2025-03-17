using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.FileLoading;
using BabelRush.Registering.I18n;
using BabelRush.Registering.RootLoaders;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Data.Registering;
using KirisameLib.Extensions;

using NLua;

using Tomlyn.Syntax;

namespace BabelRush.Registering;

public static class MakeRegistrant
{
    public static IRegistrant<TItem> ForScript<TItem, TModel>(string path) where TModel : IModel<ScriptSourceInfo, TItem> =>
        new MergedRegistrant<TItem>(
            ScriptRootLoader.WithSourceTaker(path, new RegistrantSourceTaker<ScriptSourceInfo, TModel, TItem>()),
            ManualRegistrant.Common<TItem>(RootNames.Script, path)
        );

    public static IRegistrant<TItem> ForData<TItem, TModel>(string path) where TModel : IModel<DocumentSyntax, TItem> =>
        new MergedRegistrant<TItem>(
            DataRootLoader.WithSourceTaker(path, new RegistrantSourceTaker<DocumentSyntax, TModel, TItem>()),
            ManualRegistrant.Common<TItem>(RootNames.Data, path)
        );

    public static IRegistrant<TItem> ForCommonRes<TItem, TModel>(string path) where TModel : IModel<ResSourceInfo, TItem> =>
        new MergedRegistrant<TItem>(
            ResRootLoader.WithStaticSourceTaker(path, new RegistrantSourceTaker<ResSourceInfo, TModel, TItem>()),
            ManualRegistrant.Common<TItem>(RootNames.Res, path)
        );

    public static II18nRegistrant<TItem> ForLocalRes<TItem, TModel>(string path) where TModel : IModel<ResSourceInfo, TItem> =>
        new MergedI18nRegistrant<TItem>(
            LocalFileLoader.WithResSourceTakerFactory(path, new I18nSourceTakerRegistrant<ResSourceInfo, TModel, TItem>(
                                                          RegisterEventSource.LocalRegisterDone)),
            ManualRegistrant.I18n<TItem>(RootNames.Res, path)
        );

    public static II18nRegistrant<TItem> ForLang<TItem, TModel>(string path) where TModel : IModel<IDictionary<string, object>, TItem> =>
        new MergedI18nRegistrant<TItem>(
            LocalFileLoader.WithLangSourceTakerFactory(path, new I18nSourceTakerRegistrant<IDictionary<string, object>, TModel, TItem>(
                                                           RegisterEventSource.LocalRegisterDone)),
            ManualRegistrant.I18n<TItem>(RootNames.Lang, path)
        );
}

file class MergedRegistrant<TItem>(params IRegistrant<TItem>[] registrants) : IRegistrant<TItem>
{
    public void AcceptTarget(IRegTarget<TItem> target) => registrants.ForEach(r => r.AcceptTarget(target));
}

// ReSharper disable once InconsistentNaming
file class MergedI18nRegistrant<TItem>(params II18nRegistrant<TItem>[] registrants) : II18nRegistrant<TItem>
{
    public void AcceptTarget(II18nRegTarget<TItem> target) => registrants.ForEach(r => r.AcceptTarget(target));
}