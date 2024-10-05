using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using KirisameLib.Core.Extensions;
using KirisameLib.Core.Logging;

namespace BabelRush.Registering;

public static class Registration
{
    #region RegisteringTasks

    private static ConcurrentDictionary<string, TaskCompletionSource> RegisteringTasks { get; } = [];

    #endregion


    #region Delegates & Register Methods
    
    public delegate Task ParseAndRegisterDelegate(object source);

    public delegate Task LocalizedParseAndRegisterDelegate(string local, object source);

    
    public static ParseAndRegisterDelegate GetRegFunc<TSource, TData>
        (Func<TSource, TData> parse, Action<TData> register, params string[] waitForRegisters)
        where TData : notnull => async source =>
    {
        await Task.Yield();
        var items = ParseCollection(parse, source);
        if (items.Count == 0) return;

        await Task.WhenAll(waitForRegisters.Select(s => RegisteringTasks.GetOrAdd(s, _ => new()).Task));
        foreach (var item in items)
        {
            register(item);
        }
    };

    public static ParseAndRegisterDelegate GetRegFunc<TSource, TData>
        (Func<TSource, (string id, TData data)> parse, Action<string, TData> register, params string[] waitForRegisters)
        where TData : notnull =>
        GetRegFunc(parse, t => register(t.id, t.data), waitForRegisters);

    public static LocalizedParseAndRegisterDelegate GetLocalizedRegFunc<TSource, TData>
        (Func<TSource, TData> parse, Action<string, TData> register)
        where TData : notnull => async (local, source) =>
    {
        await Task.Yield();
        var items = ParseCollection(parse, source);
        if (items.Count == 0) return;

        foreach (var item in items)
        {
            register(local, item);
        }
    };

    public static LocalizedParseAndRegisterDelegate GetLocalizedRegFunc<TSource, TData>
        (Func<TSource, (string id, TData data)> parse, Action<string, string, TData> register)
        where TData : notnull =>
        GetLocalizedRegFunc(parse, (local, t) => register(local, t.id, t.data));

    
    private static Queue<TData> ParseCollection<TSource, TData>(Func<TSource, TData> parse, object source)
        where TData : notnull
    {
        const string loggingProcessNameParsing = "ParsingAssets";
        if (source is not IEnumerable enumerable)
        {
            Logger.Log(LogLevel.Error, loggingProcessNameParsing, "Source is not IEnumerable, register skipped");
            return [];
        }
        Queue<TData> items = [];
        foreach (var entry in enumerable)
        {
            try
            {
                var item = parse((TSource)entry);
                items.Enqueue(item);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, loggingProcessNameParsing, $"Exception on Parsing, entry skipped: {e}");
            }
        }
        return items;
    }

    #endregion


    #region Register Maps

    private static Dictionary<string, ParseAndRegisterDelegate> RegMap { get; } = new();
    private static Dictionary<string, LocalizedParseAndRegisterDelegate> LocalRegMap { get; } = new();

    public static bool RegisterMap(string path, ParseAndRegisterDelegate parseAndRegister) =>
        RegMap.TryAdd(path, parseAndRegister);

    public static bool RegisterMap(string path, LocalizedParseAndRegisterDelegate parseAndRegister) =>
        LocalRegMap.TryAdd(path, parseAndRegister);

    #endregion


    //Public Methods
    public static void RegisterAssets(string[] path, object assets)
    {
        switch (path)
        {
            case ["data", ..] or ["res", ..]:
                var pathStr = path.Join('/');
                if (RegMap.TryGetValue(pathStr, out var register))
                    register(assets).ContinueWith(_ => RegisteringTasks.GetOrAdd(pathStr, _ => new()).SetResult());
                break;
            case ["local", var local, .. var rest]
                when rest is ["lang", ..] or ["res", ..]:
                if (LocalRegMap.TryGetValue(rest.Join('/'), out var localRegister))
                    localRegister(local, assets);
                break;
            default:
                Logger.Log(LogLevel.Warning, nameof(RegisterAssets), $"Invalid asset path: {path.Join('/')}");
                break;
        }
    }


    //Logging
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(Registration));
}