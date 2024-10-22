using System.Collections.Generic;

using KirisameLib.Data.FileLoading;

using Tomlyn;
using Tomlyn.Model;

namespace BabelRush.Registering;

public class LangRootLoader : RootLoader<IDictionary<string, object>, LangRegistrant>
{
    private bool Exited { get; set; }
    private LinkedList<string> SubPathList { get; } = [];
    private Dictionary<string, List<IDictionary<string, object>>> SourceDict { get; } = new();


    public override bool EnterDirectory(string dirName)
    {
        RootLoaderExitedException.ThrowIf(Exited);

        SubPathList.AddLast(dirName);
        return true;
    }

    public override void LoadFile(string fileName, byte[] fileContent)
    {
        RootLoaderExitedException.ThrowIf(Exited);

        //todo:Schema检验
        var table = Toml.Parse(fileContent).ToModel();
        foreach (var (key, value) in table)
        {
            if (value is not TomlTable source)
            {
                //todo: log here
                continue;
            }
            if (!PathMapView.ContainsKey(key))
            {
                //todo: and here
                continue;
            }
            if (!SourceDict.TryGetValue(key, out var list)) SourceDict[key] = list = [];
            list.Add(source);
        }
    }

    public override bool ExitDirectory()
    {
        RootLoaderExitedException.ThrowIf(Exited);

        if (SubPathList.Count == 0)
        {
            Register();
            Exited = true;
            return true;
        }
        SubPathList.RemoveLast();
        return false;
    }

    private void Register()
    {
        //todo: do this
    }
}