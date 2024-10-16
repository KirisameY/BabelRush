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
    protected Queue<TBox> ParseSet<TSource, TBox, TAsset>(IEnumerable<TSource> source)
        where TBox : IBox<TSource, TBox, TAsset>
    {
        const string loggingProcessNameParsing = "ParsingAssets";
        Queue<TBox> items = [];
        foreach (var entry in source)
        {
            try
            {
                var item = TBox.FromEntry(entry);
                if (item is not null)
                    items.Enqueue(item);
                else
                    Logger.Log(LogLevel.Error, loggingProcessNameParsing, $"Entry {entry} is not valid, parse skipped");
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, loggingProcessNameParsing, $"Exception on Parsing, parse of entry {e} skipped");
            }
        }
        return items;
    }


    //Logging
    protected abstract Logger Logger { get; }
}