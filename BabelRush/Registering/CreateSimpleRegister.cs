using System;
using System.Collections.Generic;

using BabelRush.Data;
using BabelRush.Registering.I18n;

using KirisameLib.Data.Registering;
using KirisameLib.Data.Registers;

using Tomlyn.Syntax;

namespace BabelRush.Registering;

public static class CreateSimpleRegister
{
    public static IEnumerableRegister<RegKey, TItem> Script<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        Script<TItem, TModel>(path, _ => fallback);

    public static IEnumerableRegister<RegKey, TItem> Script<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        new RegisterBuilder<RegKey, TItem>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(MakeRegistrant.ForScript<TItem, TModel>(path))
           .WithFallback(fallback)
           .Build();


    public static IEnumerableRegister<RegKey, TItem> Data<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, _ => fallback);

    public static IEnumerableRegister<RegKey, TItem> Data<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        new RegisterBuilder<RegKey, TItem>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(MakeRegistrant.ForData<TItem, TModel>(path))
           .WithFallback(fallback)
           .Build();


    public static I18nRegister<TItem> Res<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, _ => fallback);

    public static I18nRegister<TItem> Res<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeRegistrant.ForLocalRes<TItem, TModel>(path))
           .WithFallback(new RegisterBuilder<RegKey, TItem>()
                        .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
                        .AddRegistrant(MakeRegistrant.ForCommonRes<TItem, TModel>(path))
                        .WithFallback(fallback)
                        .Build())
           .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
           .Build();


    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, TItem fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, _ => fallback);

    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, Func<RegKey, TItem> fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeRegistrant.ForLang<TItem, TModel>(path))
           .WithDefaultLocal(defaultLocal)
           .WithFallback(fallback)
           .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
           .Build();
}