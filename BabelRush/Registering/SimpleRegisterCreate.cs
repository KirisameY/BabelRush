using System;
using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;

using Tomlyn.Syntax;

namespace BabelRush.Registering;

public static class SimpleRegisterCreate
{
    public static IEnumerableRegister<TItem> Data<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, _ => fallback);

    public static IEnumerableRegister<TItem> Data<TItem, TModel>(string path, Func<string, TItem> fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        new RegisterBuilder<TItem>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(MakeFileRegistrant.ForData<TItem, TModel>(path))
           .WithFallback(fallback)
           .Build();


    public static I18nRegister<TItem> Res<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, _ => fallback);

    public static I18nRegister<TItem> Res<TItem, TModel>(string path, Func<string, TItem> fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeFileRegistrant.ForLocalRes<TItem, TModel>(path))
           .WithFallback(new RegisterBuilder<TItem>()
                        .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
                        .AddRegistrant(MakeFileRegistrant.ForCommonRes<TItem, TModel>(path))
                        .WithFallback(fallback)
                        .Build())
           .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
           .Build();


    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, TItem fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, _ => fallback);

    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, Func<string, TItem> fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeFileRegistrant.ForLang<TItem, TModel>(path))
           .WithDefaultLocal(defaultLocal)
           .WithFallback(fallback)
           .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
           .Build();
}