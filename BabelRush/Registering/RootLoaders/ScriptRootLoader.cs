using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.SourceTakers;
using BabelRush.Scripting;

using KirisameLib.Asynchronous;
using KirisameLib.Extensions;
using KirisameLib.Logging;

namespace BabelRush.Registering.RootLoaders;

internal sealed class ScriptRootLoader(string nameSpace, bool overwriting) : CommonRootLoader<ScriptSourceInfo>
{
    private static Dictionary<string, SourceTakerRegistrant<ScriptSourceInfo>> SourceTakerDict { get; } = new()
    {
        // todo: modules loader
        //["_modules"] =
    };

    public static T WithSourceTaker<T>(string path, T taker) where T : SourceTakerRegistrant<ScriptSourceInfo>
    {
        if (path.StartsWith('_'))
            throw new InvalidOperationException($"Path that begin with '_' is reserved. (Invalid path: {path})");
        if (!SourceTakerDict.TryAdd(path, taker))
            throw new InvalidOperationException($"SourceTaker for path {path} is already registered.");

        return taker;
    }

    protected override ISourceTaker<ScriptSourceInfo>? GetSourceTaker(string path) =>
        SourceTakerDict.GetOrDefault(path)?.CreateSourceTaker(nameSpace, overwriting);

    protected override void HandleFile(Dictionary<string, ScriptSourceInfo> sourceDict, string[] fileSubPath, byte[] fileContent)
    {
        var extension = Path.GetExtension(fileSubPath.Last());
        var path = fileSubPath.SkipLast(1).Append(Path.ChangeExtension(fileSubPath.Last(), null)).ToImmutableArray();
        var pathString = path.Join('/');
        if (extension is not ".lua")
        {
            Logger.Log(LogLevel.Warning, nameof(HandleFile),
                       $"Unexpected file type {extension} in Script/{CurrentPath}/{pathString}{extension}");
            return;
        }

        var function = ScriptHub.Lua.LoadString(fileContent, "chunk");
        sourceDict.TryAdd(pathString, new(path, function));
    }

    private readonly TaskCompletionSource _startRegistering = new();

    protected override async Task RegisterDirectory(ISourceTaker<ScriptSourceInfo> sourceTaker, Dictionary<string, ScriptSourceInfo> sourceDict)
    {
        var path = CurrentPath;

        await AsyncOrrery.SwitchContext();
        await _startRegistering.Task;

        foreach (var (file, source) in sourceDict)
        {
            sourceTaker.Take(source, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in Script/{path}/{file}, error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }
        }
    }

    protected override void EndUp(Task registeringTask)
    {
        _startRegistering.SetResult();
    }


    // Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(ScriptRootLoader));
}