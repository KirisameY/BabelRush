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
    // Script
    public static IEnumerableRegister<RegKey, TItem> Script<TItem, TModel>(string path, TItem fallback)
        where TItem : notnull
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        Script<TItem, TModel>(path, fallback, false);

    public static IEnumerableRegister<RegKey, TItem> ScriptNullable<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        Script<TItem, TModel>(path, fallback, true);

    public static IEnumerableRegister<RegKey, TItem> Script<TItem, TModel>(string path, TItem fallback, bool nullable)
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        Script<TItem, TModel>(path, _ => fallback, nullable);

    public static IEnumerableRegister<RegKey, TItem> Script<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TItem : notnull
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        Script<TItem, TModel>(path, fallback, false);

    public static IEnumerableRegister<RegKey, TItem> ScriptNullable<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        Script<TItem, TModel>(path, fallback, true);

    public static IEnumerableRegister<RegKey, TItem> Script<TItem, TModel>(string path, Func<RegKey, TItem> fallback, bool nullable)
        where TModel : IModel<ScriptSourceInfo, TItem> =>
        new RegisterBuilder<RegKey, TItem>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(MakeRegistrant.ForScript<TItem, TModel>(path))
           .WithFallback(fallback)
           .Build()
           .AddToRegisterHub(path, nullable);


    // Data
    public static IEnumerableRegister<RegKey, TItem> Data<TItem, TModel>(string path, TItem fallback)
        where TItem : notnull
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, fallback, false);

    public static IEnumerableRegister<RegKey, TItem> DataNullable<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, fallback, true);

    public static IEnumerableRegister<RegKey, TItem> Data<TItem, TModel>(string path, TItem fallback, bool nullable)
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, _ => fallback, nullable);

    public static IEnumerableRegister<RegKey, TItem> Data<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TItem : notnull
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, fallback, false);

    public static IEnumerableRegister<RegKey, TItem> DataNullable<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TModel : IModel<DocumentSyntax, TItem> =>
        Data<TItem, TModel>(path, fallback, true);

    public static IEnumerableRegister<RegKey, TItem> Data<TItem, TModel>(string path, Func<RegKey, TItem> fallback, bool nullable)
        where TModel : IModel<DocumentSyntax, TItem> =>
        new RegisterBuilder<RegKey, TItem>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(MakeRegistrant.ForData<TItem, TModel>(path))
           .WithFallback(fallback)
           .Build()
           .AddToRegisterHub(path, nullable);


    // CommonRes
    public static IEnumerableRegister<RegKey, TItem> CommonRes<TItem, TModel>(string path, TItem fallback)
        where TItem : notnull
        where TModel : IModel<ResSourceInfo, TItem> =>
        CommonRes<TItem, TModel>(path, fallback, false);

    public static IEnumerableRegister<RegKey, TItem> CommonResNullable<TItem, TModel>(string path, TItem fallback)
        where TItem : notnull
        where TModel : IModel<ResSourceInfo, TItem> =>
        CommonRes<TItem, TModel>(path, fallback, true);

    public static IEnumerableRegister<RegKey, TItem> CommonRes<TItem, TModel>(string path, TItem fallback, bool nullable)
        where TModel : IModel<ResSourceInfo, TItem> =>
        CommonRes<TItem, TModel>(path, _ => fallback, nullable);

    public static IEnumerableRegister<RegKey, TItem> CommonRes<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TItem : notnull
        where TModel : IModel<ResSourceInfo, TItem> =>
        CommonRes<TItem, TModel>(path, fallback, false);

    public static IEnumerableRegister<RegKey, TItem> CommonResNullable<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TItem : notnull
        where TModel : IModel<ResSourceInfo, TItem> =>
        CommonRes<TItem, TModel>(path, fallback, true);

    public static IEnumerableRegister<RegKey, TItem> CommonRes<TItem, TModel>(string path, Func<RegKey, TItem> fallback, bool nullable)
        where TModel : IModel<ResSourceInfo, TItem> =>
        new RegisterBuilder<RegKey, TItem>()
           .WithRegisterDoneEventSource(RegisterEventSource.CommonRegisterDone)
           .AddRegistrant(MakeRegistrant.ForCommonRes<TItem, TModel>(path))
           .WithFallback(fallback)
           .Build()
           .AddToRegisterHub(path, nullable);


    // Res
    public static I18nRegister<TItem> Res<TItem, TModel>(string path, TItem fallback)
        where TItem : notnull
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, fallback, false);

    public static I18nRegister<TItem> ResNullable<TItem, TModel>(string path, TItem fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, fallback, true);

    public static I18nRegister<TItem> Res<TItem, TModel>(string path, TItem fallback, bool nullable)
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, _ => fallback, nullable);

    public static I18nRegister<TItem> Res<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TItem : notnull
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, fallback, false);

    public static I18nRegister<TItem> ResNullable<TItem, TModel>(string path, Func<RegKey, TItem> fallback)
        where TModel : IModel<ResSourceInfo, TItem> =>
        Res<TItem, TModel>(path, fallback, true);

    public static I18nRegister<TItem> Res<TItem, TModel>(string path, Func<RegKey, TItem> fallback, bool nullable)
        where TModel : IModel<ResSourceInfo, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeRegistrant.ForLocalRes<TItem, TModel>(path))
           .WithFallback(CommonRes<TItem, TModel>(path, fallback, nullable))
           .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
           .Build()
           .AddToRegisterHub(path, nullable);


    // Lang
    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, TItem fallback)
        where TItem : notnull
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, fallback, false);

    public static I18nRegister<TItem> LangNullable<TItem, TModel>(string path, string defaultLocal, TItem fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, fallback, true);

    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, TItem fallback, bool nullable)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, _ => fallback, nullable);

    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, Func<RegKey, TItem> fallback)
        where TItem : notnull
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, fallback, false);

    public static I18nRegister<TItem> LangNullable<TItem, TModel>(string path, string defaultLocal, Func<RegKey, TItem> fallback)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        Lang<TItem, TModel>(path, defaultLocal, fallback, true);

    public static I18nRegister<TItem> Lang<TItem, TModel>(string path, string defaultLocal, Func<RegKey, TItem> fallback, bool nullable)
        where TModel : IModel<IDictionary<string, object>, TItem> =>
        new I18nRegisterBuilder<TItem>()
           .WithRegistrant(MakeRegistrant.ForLang<TItem, TModel>(path))
           .WithDefaultLocal(defaultLocal)
           .WithFallback(fallback)
           .WithRegisterDoneEventSource(RegisterEventSource.LocalRegisterDone)
           .Build()
           .AddToRegisterHub(path, nullable);
}