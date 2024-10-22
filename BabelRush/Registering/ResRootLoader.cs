using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using BabelRush.Data;

using KirisameLib.Core.Extensions;
using KirisameLib.Core.Logging;
using KirisameLib.Data.FileLoading;
using KirisameLib.Data.Registration;

namespace BabelRush.Registering;

public class ResRootLoader : CommonRootLoader<ResSourceInfo, Registrant<ResSourceInfo>>
{
    protected override void HandleFile(Dictionary<string, ResSourceInfo> sourceDict, string fileSubPath, byte[] fileContent)
    {
        var extension = Path.GetExtension(fileSubPath);
        var name = fileSubPath.Remove(fileSubPath.Length - extension.Length);

        if (!sourceDict.TryGetValue(name, out var source))
            sourceDict.Add(name, source = new ResSourceInfo(name));

        source.Files.TryAdd(extension, fileContent);
    }

    protected override async Task RegisterDirectory(Registrant<ResSourceInfo> registrant, Dictionary<string, ResSourceInfo> sourceDict)
    {
        var path = CurrentPath;
        await Task.Yield();

        foreach (var source in sourceDict.Values)
        {
            var regInfos = registrant.Parse(source, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in Res/{path}/{source.Id}, error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }

            foreach (var (id, register) in regInfos)
            {
                if (register()) continue;

                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"Failed to register item {id} in Res/{path},"
                         + $"Possibly there's already a registered item with a duplicate ID.");
            }
        }
    }

    protected override void EndUp() { }


    //Logger
    private static Logger Logger { get; } = LogManager.GetLogger(nameof(ResRootLoader));
}