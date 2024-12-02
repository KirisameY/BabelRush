using System.Collections.Generic;
using System.Linq;

using KirisameLib.Core.Extensions;
using KirisameLib.Data.FileLoading;
using KirisameLib.Logging;

using Tomlyn;
using Tomlyn.Model;

namespace BabelRush.Registering;

public class LangRootLoader : RootLoader<IDictionary<string, object>, LangRegistrant>
{
    private bool Exited { get; set; }
    private LinkedList<string> SubPathLink { get; } = [];


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
            if (!PathMapView.ContainsKey(key))
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
        var registrant = PathMapView[sort];

        var regInfos = registrant.Parse(source, out var errorInfo);
        if (errorInfo.ErrorCount != 0)
        {
            Logger.Log(LogLevel.Warning, nameof(Register),
                       $"{errorInfo.ErrorCount} errors found in data sort {sort} of Lang/{filePath} (in {FileLoader.CurrentLocalInfo}), "
                     + $"error messages:\n"
                     + errorInfo.Messages.Join('\n'));
        }

        foreach (var (id, register) in regInfos)
        {
            if (register()) continue;
            Logger.Log(LogLevel.Warning, nameof(Register),
                       $"Failed to register item {id} in data sort {sort} of Lang/{filePath} (in {FileLoader.CurrentLocalInfo}), "
                     + $"Possibly there's already a registered item with a duplicate ID.");
        }
    }


    //Logging
    private Logger Logger { get; } = Game.LogBus.GetLogger(nameof(LangRootLoader));
}