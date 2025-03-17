using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using BabelRush.Registering.SourceTakers;

using KirisameLib.Extensions;
using KirisameLib.Logging;

using Tomlyn;
using Tomlyn.Model;

namespace BabelRush.Registering.RootLoaders;

internal sealed class LangRootLoader(string local, IDictionary<string, ISourceTaker<IDictionary<string, object>>> sourceTakerDict) :
    RootLoader<IDictionary<string, object>>
{
    private string Local { get; } = local;
    private ImmutableDictionary<string, ISourceTaker<IDictionary<string, object>>> SourceTakerDict { get; } =
        sourceTakerDict.ToImmutableDictionary();

    private bool Exited { get; set; }
    private LinkedList<string> SubPathLink { get; } = [];


    protected override ISourceTaker<IDictionary<string, object>>? GetSourceTaker(string path)
    {
        return SourceTakerDict.GetOrDefault(path);
    }

    public override bool EnterDirectory(string dirName)
    {
        RootLoaderExitedException.ThrowIf(Exited);

        SubPathLink.AddLast(dirName);
        return true;
    }

    public override void LoadFile(string fileName, byte[] fileContent)
    {
        RootLoaderExitedException.ThrowIf(Exited);

        var filePath = SubPathLink.Append(fileName).Join('/');
        var table = Toml.Parse(fileContent).ToModel();
        foreach (var (key, value) in table)
        {
            if (value is not TomlTable source)
            {
                Logger.Log(LogLevel.Warning, nameof(LoadFile),
                           $"Source data {key} in file {filePath} is not a table, skipped");
                continue;
            }
            if (!SourceTakerDict.ContainsKey(key))
            {
                Logger.Log(LogLevel.Warning, nameof(LoadFile),
                           $"Source data {key} in file {filePath} is unrecognized, skipped");
                continue;
            }

            Register(fileName, key, source);
        }
    }

    public override bool ExitDirectory()
    {
        RootLoaderExitedException.ThrowIf(Exited);

        if (SubPathLink.Count != 0)
        {
            SubPathLink.RemoveLast();
            return false;
        }

        Exited = true;
        return true;
    }

    private void Register(string filePath, string sort, IDictionary<string, object> source)
    {
        if (GetSourceTaker(sort) is not { } registrant) return;

        registrant.Take(source, out var errorInfo);
        if (errorInfo.ErrorCount != 0)
        {
            Logger.Log(LogLevel.Warning, nameof(Register),
                       $"{errorInfo.ErrorCount} errors found in data sort {sort} of Lang/{filePath} (in {Local}), "
                     + $"error messages:\n" + errorInfo.Messages.Join('\n'));
        }
    }


    //Logging
    private Logger Logger { get; } = Game.LogBus.GetLogger(nameof(LangRootLoader));
}