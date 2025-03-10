using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using BabelRush.Data;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Asynchronous;
using KirisameLib.Data.Registering;
using KirisameLib.Extensions;
using KirisameLib.Logging;

using Tomlyn;
using Tomlyn.Syntax;

namespace BabelRush.Registering.RootLoaders;

internal class DataRootLoader : CommonRootLoader<DocumentSyntax>
{
    private static Dictionary<string, ISourceTaker<DocumentSyntax>> SourceTakerDict { get; } = new();

    public static IRegistrant<TItem> NewRegistrant<TItem, TModel>(string path)
        where TModel : IModel<DocumentSyntax, TItem>
    {
        var taker = new RegistrantSourceTaker<DocumentSyntax, TModel, TItem>();
        if (!SourceTakerDict.TryAdd(path, taker))
        {
            throw new InvalidOperationException($"SourceTaker for path {path} is already registered.");
        }
        return taker;
    }

    protected override ISourceTaker<DocumentSyntax>? GetSourceTaker(string path) => SourceTakerDict.GetOrDefault(path);

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

    protected override async Task RegisterDirectory(ISourceTaker<DocumentSyntax> sourceTaker, Dictionary<string, DocumentSyntax> sourceDict)
    {
        var path = CurrentPath;
        await AsyncOrrery.SwitchContext();

        foreach (var (file, source) in sourceDict)
        {
            sourceTaker.Take(source, out var errorInfo);
            if (errorInfo.ErrorCount != 0)
            {
                Logger.Log(LogLevel.Warning, nameof(RegisterDirectory),
                           $"{errorInfo.ErrorCount} errors found in Data/{path}/{file}, error messages:\n"
                         + errorInfo.Messages.Join('\n'));
            }
        }
    }

    protected override void EndUp() { }


    // New Registrant


    // Logging
    private static Logger Logger { get; } = Game.LogBus.GetLogger(nameof(DataRootLoader));
}