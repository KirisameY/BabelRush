using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using KirisameLib.Core.Logging;

namespace BabelRush.Registering;

public abstract class AssetRegTool(string path)
{
    public string Path { get; } = path;

    
    //Protected Methods
    protected Queue<TData> ParseSet<TSource, TData>(Func<TSource, TData> parse, object source)
        where TData : notnull
    {
        const string loggingProcessNameParsing = "ParsingAssets";
        if (source is not IEnumerable enumerable)
        {
            Logger.Log(LogLevel.Error, loggingProcessNameParsing, $"Source {source} is not IEnumerable, register skipped");
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

    
    //Public Methods(abstract)
    public abstract Task RegisterSet(object source);
    public abstract Task RegisterLocalizedSet(string local,object source);
    
    
    //Logging
    protected abstract Logger Logger { get; }
}