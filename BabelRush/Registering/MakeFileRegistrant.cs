using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.FileLoading;
using BabelRush.Registering.I18n;
using BabelRush.Registering.RootLoaders;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Data.Registering;

using Tomlyn.Syntax;

namespace BabelRush.Registering;

public static class MakeFileRegistrant
{
    public static IRegistrant<TItem> ForData<TItem, TModel>(string path) where TModel : IModel<DocumentSyntax, TItem> =>
        DataRootLoader.WithSourceTaker(path, new RegistrantSourceTaker<DocumentSyntax, TModel, TItem>());

    public static IRegistrant<TItem> ForCommonRes<TItem, TModel>(string path) where TModel : IModel<ResSourceInfo, TItem> =>
        ResRootLoader.WithStaticSourceTaker(path, new RegistrantSourceTaker<ResSourceInfo, TModel, TItem>());

    public static II18nRegistrant<TItem> ForLocalRes<TItem, TModel>(string path) where TModel : IModel<ResSourceInfo, TItem> =>
        LocalFileLoader.WithResSourceTakerFactory(path, new I18nSourceTakerRegistrant<ResSourceInfo, TModel, TItem>(RegisterEventSource.LocalRegisterDone));

    public static II18nRegistrant<TItem> ForLang<TItem, TModel>(string path) where TModel : IModel<IDictionary<string, object>, TItem> =>
        LocalFileLoader.WithLangSourceTakerFactory(path, new I18nSourceTakerRegistrant<IDictionary<string, object>, TModel, TItem>(RegisterEventSource.LocalRegisterDone));
}