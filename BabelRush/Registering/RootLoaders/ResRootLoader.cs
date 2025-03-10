using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Asynchronous;
using KirisameLib.Data.Registering;
using KirisameLib.Extensions;
using KirisameLib.Logging;

namespace BabelRush.Registering.RootLoaders;

public class ResRootLoader((string local, IDictionary<string, ISourceTaker<ResSourceInfo>> dict)? localInfo = null) : CommonRootLoader<ResSourceInfo>
{
    internal static IRegistrant<TItem> NewRegistrant<TItem, TModel>(string path)
        where TModel : IModel<ResSourceInfo, TItem>
    {
        var taker = new RegistrantSourceTaker<ResSourceInfo, TModel, TItem>();
        if (!StaticSourceTakerDict.TryAdd(path, taker))
        {
            throw new InvalidOperationException($"SourceTaker for path {path} is already registered.");
        }
        return taker;
    }

    private static Dictionary<string, ISourceTaker<ResSourceInfo>> StaticSourceTakerDict { get; } = new();
    private string LocalInfo { get; } = localInfo?.local ?? "common res";
    private ImmutableDictionary<string, ISourceTaker<ResSourceInfo>> SourceTakerDict { get; } =
        (localInfo?.dict ?? StaticSourceTakerDict).ToImmutableDictionary();


    protected override ISourceTaker<ResSourceInfo>? GetSourceTaker(string path) => SourceTakerDict.GetOrDefault(path);

    protected override void HandleFile(Dictionary<string, ResSourceInfo> sourceDict, string fileSubPath, byte[] fileContent)
    {
        var extension = Path.GetExtension(fileSubPath);
        var pathName = fileSubPath.Remove(fileSubPath.Length - extension.Length);
        var name = Path.GetFileNameWithoutExtension(fileSubPath);

        if (!sourceDict.TryGetValue(pathName, out var source))
            sourceDict.Add(pathName, source = new ResSourceInfo(name));

        source.Files.TryAdd(extension, fileContent);
    }

    protected override async Task RegisterDirectory(ISourceTaker<ResSourceInfo> sourceTaker, Dictionary<string, ResSourceInfo> sourceDict)
    {
        var path = CurrentPath;
        await AsyncOrrery.SwitchContext();

        foreach (var source in sourceDict.Values)
        {
            sourceTaker.Take(source, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in Res/{path}/{source.Id} (in {LocalInfo}), "
                         + $"error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }
        }
    }

    protected override void EndUp() { }


    //Logger
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(ResRootLoader));
}