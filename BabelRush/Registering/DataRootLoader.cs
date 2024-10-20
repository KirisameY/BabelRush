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
        await Task.WhenAll(info.Registrant.WaitFor.Select(wait => TaskDict.GetOrAdd(wait, _ => new()).Task));

        foreach (var (file, source) in info.SourceDict)
        {
            var regInfos = info.Registrant.Parse(source, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in {info.Path}/{file}, error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }

            foreach (var (id, register) in regInfos)
            {
                if (register()) continue;
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"Failed to register item {id} in {info.Path}/{file},"
                         + $"Possibly there's already a registered item with a duplicate ID.");
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