using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.Parsing;

using KirisameLib.Core.Logging;

namespace BabelRush.Registering;

public abstract class AssetRegTool(string path)
{
    public string Path { get; } = path;


    //Protected Methods
    protected Queue<TBox> ParseSet<TSource, TBox, TAsset>(IEnumerable source)
        where TBox : IBox<TSource, TBox, TAsset>
    {
        const string loggingProcessNameParsing = "ParsingAssets";
        if (source is not IEnumerable<TSource> enumerable)
        {
            Logger.Log(LogLevel.Error, loggingProcessNameParsing,
                       $"Source {source} is not an IEnumerable of {typeof(TSource)}, register skipped");
            return [];
        }
        Queue<TBox> items = [];
        foreach (var entry in enumerable)
        {
            try
            {
                var item = TBox.FromEntry(entry);
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
    public abstract Task RegisterSet(IEnumerable source);
    public abstract Task RegisterLocalizedSet(string local, IEnumerable source);


    //Logging
    protected abstract Logger Logger { get; }
}