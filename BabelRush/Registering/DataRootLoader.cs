using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using KirisameLib.Extensions;
using KirisameLib.Data.FileLoading;
using KirisameLib.Logging;

using Tomlyn;
using Tomlyn.Syntax;

namespace BabelRush.Registering;

public class DataRootLoader : CommonRootLoader<DocumentSyntax, DataRegistrant>
{
    private static ConcurrentDictionary<string, TaskCompletionSource> TaskDict { get; } = new();

    protected override void HandleFile(Dictionary<string, DocumentSyntax> sourceDict, string fileSubPath, byte[] fileContent)
    {
        var extension = Path.GetExtension(fileSubPath);
        if (extension != ".toml")
        {
            Logger.Log(LogLevel.Warning, nameof(HandleFile),
                       $"Unexpected file type {extension} in Data/{CurrentPath}/{fileSubPath}");
            return;
        }
        var syntax = Toml.Parse(fileContent);
        sourceDict.TryAdd(fileSubPath, syntax);
    }

    protected override async Task RegisterDirectory(DataRegistrant registrant, Dictionary<string, DocumentSyntax> sourceDict)
    {
        var path = CurrentPath;
        await Task.WhenAll(registrant.WaitFor.Select(wait => TaskDict.GetOrAdd(wait, _ => new()).Task));

        foreach (var (file, source) in sourceDict)
        {
            var regInfos = registrant.Parse(source, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in Data/{path}/{file}, error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }

            foreach (var (id, register) in regInfos)
            {
                if (register()) continue;
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"Failed to register item {id} in Data/{path}/{file},"
                         + $"Possibly there's already a registered item with a duplicate ID.");
            }
        }

        TaskDict.GetOrAdd(path, _ => new()).SetResult();
    }

    protected override void EndUp()
    {
        Task.WhenAll(TaskDict.Values.Select(x => x.Task)).Wait();
    }


    //Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(DataRootLoader));
}