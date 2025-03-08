using System.Collections.Generic;
using System.Linq;

using BabelRush.Registering.RootLoaders;
using BabelRush.Registering.SourceTakers;

using KirisameLib.Extensions;

namespace BabelRush.Registering;

public static class FileLoader
{
    #region Map

    private static DataRootLoader DataRoot { get; } = new();
    private static ResRootLoader DefaultResRoot { get; } = new();
    private static ResRootLoader LocalResRoot { get; } = new();
    private static LangRootLoader LangRoot { get; } = new();

    private static Dictionary<string, RootLoader?> RootMap { get; } = new()
    {
        ["data"] = DataRoot,
        // ["script"] = null, todo: script也该安排了
        ["res"] = DefaultResRoot,
    };
    private static Dictionary<string, RootLoader?> LocalRootMap { get; } = new()
    {
        ["res"] = LocalResRoot,
        ["lang"] = LangRoot,
    };

    public static void AddDataRegistrant(string path, DataSourceTaker sourceTaker) => DataRoot.AddRegistrant(path, sourceTaker);
    public static void AddDefaultResRegistrant(string path, ResSourceTaker sourceTaker) => DefaultResRoot.AddRegistrant(path, sourceTaker);
    public static void AddLocalResRegistrant(string path, ResSourceTaker sourceTaker) => LocalResRoot.AddRegistrant(path, sourceTaker);
    public static void AddLangRegistrant(string path, LangSourceTaker sourceTaker) => LangRoot.AddRegistrant(path, sourceTaker);

    #endregion


    #region State

    private static LinkedList<string> DirectoryStack { get; } = [];
    private static RootLoader? CurrentRootLoader { get; set; }
    internal static string CurrentLocal { get; private set; } = "";
    internal static string CurrentLocalInfo => CurrentLocal == "" ? "general directory" : $"local: {CurrentLocal}";

    #endregion


    #region Public Methods

    public static void EnterDirectory(string dirName)
    {
        if (CurrentRootLoader is not null)
        {
            CurrentRootLoader.EnterDirectory(dirName);
            return;
        }

        DirectoryStack.AddLast(dirName);
        if (DirectoryStack.ToList() is ["local", var local, .. var rest])
        {
            CurrentLocal = local;
            LocalRootMap.TryGetValue(rest.Join('/'), out var loader);
            CurrentRootLoader = loader;
        }
        else
        {
            CurrentLocal = "";
            RootMap.TryGetValue(DirectoryStack.Join('/'), out var loader);
            CurrentRootLoader = loader;
        }
    }

    public static void ExitDirectory()
    {
        if (DirectoryStack.Count == 0) return;

        if (CurrentRootLoader is null)
        {
            DirectoryStack.RemoveLast();
            return;
        }

        if (CurrentRootLoader.ExitDirectory())
        {
            DirectoryStack.RemoveLast();
            CurrentRootLoader = null;
        }
    }

    public static void LoadFile(string fileName, byte[] fileContent)
    {
        CurrentRootLoader?.LoadFile(fileName, fileContent);
    }

    #endregion
}