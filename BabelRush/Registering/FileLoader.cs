using System.Collections.Generic;
using System.Linq;

using KirisameLib.Core.Extensions;
using KirisameLib.Data.FileLoading;

namespace BabelRush.Registering;

public static class FileLoader
{
    #region Map

    private static Dictionary<string, RootLoader?> RootMap { get; } = new()
    {
        [""] = null
    };
    private static Dictionary<string, RootLoader?> LocalRootMap { get; } = new()
    {
        [""] = null
    };

    internal static bool RegisterRoot(string rootPath, RootLoader rootLoader) => RootMap.TryAdd(rootPath, rootLoader);
    internal static bool RegisterLocalRoot(string rootPath, RootLoader rootLoader) => LocalRootMap.TryAdd(rootPath, rootLoader);

    #endregion


    #region State

    private static LinkedList<string> DirectoryStack { get; } = new();
    private static RootLoader? CurrentRootLoader { get; set; }
    internal static string? CurrentLocal { get; private set; }

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
            RootMap.TryGetValue(DirectoryStack.Join('/'), out var loader);
            CurrentRootLoader = loader;
        }
    }

    public static void ExitDirectory()
    {
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