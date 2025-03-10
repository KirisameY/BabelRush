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
    public static IRegister<TItem> Data<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, _ => fallback);

    public static IRegister<TItem> Data<TItem, TModel>(string path, Func<string, TItem> fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        new RegisterBuilder<TItem>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(MakeRegistrant.ForData<TItem, TModel>(path))
           .WithFallback(fallback)
           .Build();


    public static IRegister<TItem> Res<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, _ => fallback);

    public static IRegister<TItem> Res<TItem, TModel>(string path, Func<string, TItem> fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeRegistrant.ForLocalRes<TItem, TModel>(path))
           .WithFallback(new RegisterBuilder<TItem>()
                        .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
                        .AddRegistrant(MakeRegistrant.ForCommonRes<TItem, TModel>(path))
                        .WithFallback(fallback)
                        .Build())
           .Build();


    public static IRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, TItem fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, _ => fallback);

    public static IRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, Func<string, TItem> fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeRegistrant.ForLang<TItem, TModel>(path))
           .WithDefaultLocal(defaultLocal)
           .WithFallback(fallback)
           .Build();
}