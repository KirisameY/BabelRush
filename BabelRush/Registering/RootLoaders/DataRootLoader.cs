using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using BabelRush.Registering.SourceTakers;

using KirisameLib.Asynchronous;
using KirisameLib.Extensions;
using KirisameLib.Logging;

using Tomlyn;
using Tomlyn.Syntax;

namespace BabelRush.Registering.RootLoaders;

internal sealed class DataRootLoader(string nameSpace, bool overwriting) : CommonRootLoader<DocumentSyntax>
{
    private static Dictionary<string, SourceTakerRegistrant<DocumentSyntax>> SourceTakerDict { get; } = new();

    public static T WithSourceTaker<T>(string path, T taker) where T : SourceTakerRegistrant<DocumentSyntax>
    {
        if (!SourceTakerDict.TryAdd(path, taker))
        {
            throw new InvalidOperationException($"SourceTaker for path {path} is already registered.");
        }
        return taker;
    }

    protected override ISourceTaker<DocumentSyntax>? GetSourceTaker(string path) =>
        SourceTakerDict.GetOrDefault(path)?.CreateSourceTaker(nameSpace, overwriting);

    protected override void HandleFile(Dictionary<string, DocumentSyntax> sourceDict, string[] fileSubPath, byte[] fileContent)
    {
        var path = fileSubPath.Join('/');
        var extension = Path.GetExtension(path);
        if (extension != ".toml")
        {
            Logger.Log(LogLevel.Warning, nameof(HandleFile),
                       $"Unexpected file type {extension} in Data/{CurrentPath}/{path}");
            return;
        }
        var syntax = Toml.Parse(fileContent);
        sourceDict.TryAdd(path, syntax);
    }

    protected override async Task RegisterDirectory(ISourceTaker<DocumentSyntax> sourceTaker, Dictionary<string, DocumentSyntax> sourceDict)
    {
        var path = CurrentPath;
        await AsyncOrrery.SwitchContext();

        foreach (var (file, source) in sourceDict)
        {
            sourceTaker.Take(source, CurrentPath, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in Data/{path}/{file}, error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }
        }
    }

    protected override void EndUp(Task registeringTask) { }


    // Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(DataRootLoader));
}