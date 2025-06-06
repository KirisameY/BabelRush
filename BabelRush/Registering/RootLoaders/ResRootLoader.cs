using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Asynchronous;
using KirisameLib.Extensions;
using KirisameLib.Logging;

namespace BabelRush.Registering.RootLoaders;

internal sealed class ResRootLoader(string nameSpace, bool overwriting, (string local, IDictionary<string, SourceTakerRegistrant<ResSourceInfo>> dict)? localInfo = null) : CommonRootLoader<ResSourceInfo>
{
    public static T WithStaticSourceTaker<T>(string path, T taker) where T : SourceTakerRegistrant<ResSourceInfo>
    {
        if (!StaticSourceTakerDict.TryAdd(path, taker))
        {
            throw new InvalidOperationException($"SourceTaker for path {path} is already registered.");
        }
        return taker;
    }

    private static Dictionary<string, SourceTakerRegistrant<ResSourceInfo>> StaticSourceTakerDict { get; } = new();
    private string LocalInfo { get; } = localInfo?.local ?? "common res";
    private ImmutableDictionary<string, SourceTakerRegistrant<ResSourceInfo>> SourceTakerDict { get; } =
        (localInfo?.dict ?? StaticSourceTakerDict).ToImmutableDictionary();


    protected override ISourceTaker<ResSourceInfo>? GetSourceTaker(string path) =>
        SourceTakerDict.GetOrDefault(path)?.CreateSourceTaker(nameSpace, overwriting);

    protected override void HandleFile(Dictionary<string, ResSourceInfo> sourceDict, string[] fileSubPath, byte[] fileContent)
    {
        var dir = fileSubPath.SkipLast(1).ToImmutableArray();
        if (fileSubPath.Last().Split('#', 2) is not [var name, var extension]) // todo: append texture support
        {
            name      = Path.GetFileNameWithoutExtension(fileSubPath.Last());
            extension = Path.GetExtension(fileSubPath.Last());
        }
        var pathNoExt = dir.Append(name).Join('/');

        if (!sourceDict.TryGetValue(pathNoExt, out var source))
            sourceDict.Add(pathNoExt, source = new ResSourceInfo(dir, name));

        source.Files.TryAdd(extension, fileContent);
    }

    protected override async Task RegisterDirectory(ISourceTaker<ResSourceInfo> sourceTaker, Dictionary<string, ResSourceInfo> sourceDict)
    {
        var path = CurrentPath;
        await AsyncOrrery.SwitchContext();

        foreach (var source in sourceDict.Values)
        {
            sourceTaker.Take(source, CurrentPath, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in Res/{path}/{source.Path} (in {LocalInfo}), "
                         + $"error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }
        }
    }

    protected override void EndUp(Task registeringTask) { }


    //Logger
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(ResRootLoader));
}