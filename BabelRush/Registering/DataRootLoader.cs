using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using KirisameLib.Core.Extensions;
using KirisameLib.Core.Logging;
using KirisameLib.Data.FileLoading;

using Tomlyn;

namespace BabelRush.Registering;

//todo: res和lang的版本，这个优先级最高
public class DataRootLoader : CommonRootLoader<IDictionary<string, object>, DataRegistrant>
{
    private static ConcurrentDictionary<string, TaskCompletionSource> TaskDict { get; } = new();

    protected override void HandleFile(Dictionary<string, IDictionary<string, object>> sourceDict, string fileName, byte[] fileContent)
    {
        //todo:这个也要改
        if (!fileName.ToLower().EndsWith(".toml")) return;

        var model = Toml.Parse(fileContent).ToModel();
        sourceDict.TryAdd(fileName, model);
    }

    protected override async void RegisterDirectory(RegisterInfo info)
    {
        await Task.Yield();
        Dictionary<string, (string id, Func<bool> register)[]> regInfos = [];
        foreach (var pair in info.SourceDict)
        {
            regInfos.Add(pair.Key, info.Registrant.Parse(pair.Value, out var errorInfo));
            if (errorInfo.ErrorCount == 0) continue;
            Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                       $"{errorInfo.ErrorCount} errors found in {info.Path}/{pair.Key}, error messages:\n{errorInfo.Messages.Join('\n')}");
        }

        await Task.WhenAll(info.Registrant.WaitFor.Select(wait => TaskDict.GetOrAdd(wait, _ => new()).Task));
        foreach (var (file, regSort) in regInfos)
        {
            foreach (var (id, register) in regSort)
            {
                if (register()) continue;
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"Failed to register item {id} in {info.Path}/{file},Possibly there's already a registered item with a duplicate ID.");
            }
        }
        TaskDict.GetOrAdd(info.Path, _ => new()).SetResult();
    }

    protected override void EndUp()
    {
        Task.WhenAll(TaskDict.Values.Select(x => x.Task)).Wait();
    }


    //Logging
    private Logger Logger { get; } = LogManager.GetLogger(nameof(DataRootLoader));
}