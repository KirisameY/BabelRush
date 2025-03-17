using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.FileLoading;
using BabelRush.Registering.I18n;
using BabelRush.Registering.RootLoaders;
using BabelRush.Registering.SourceTakers;

using NLua;

using Tomlyn.Syntax;

namespace BabelRush.Registering;

public static class HookSourceLoader
{
    public static T OnScript<T>(string path, T loader) where T : ISourceTaker<LuaFunction> =>
        ScriptRootLoader.WithSourceTaker(path, loader);

    public static T OnData<T>(string path, T loader) where T : ISourceTaker<DocumentSyntax> =>
        DataRootLoader.WithSourceTaker(path, loader);

    public static T OnCommonRes<T>(string path, T loader) where T : ISourceTaker<ResSourceInfo> =>
        ResRootLoader.WithStaticSourceTaker(path, loader);

    public static T OnLocalRes<T>(string path, T loader) where T : II18nSourceTakerFactory<ResSourceInfo> =>
        LocalFileLoader.WithResSourceTakerFactory(path, loader);

    public static T OnLang<T>(string path, T loader) where T : II18nSourceTakerFactory<IDictionary<string, object>> =>
        LocalFileLoader.WithLangSourceTakerFactory(path, loader);
}