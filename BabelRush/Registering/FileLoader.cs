using System;
using System.Collections.Generic;

using BabelRush.Registering.RootLoaders;

using KirisameLib.Extensions;

namespace BabelRush.Registering;

public static class FileLoader
{
    #region Map

    private static Dictionary<string, Func<RootLoader>> RootMap { get; } = new()
    {
        ["data"] = static () => new DataRootLoader(),
        // ["script"] = null, todo: script也该安排了
        ["res"] = static () => new ResRootLoader(),
    };

    #endregion


    #region State

    private static LinkedList<string> DirectoryLink { get; } = [];
    private static RootLoader? CurrentRootLoader { get; set; }

    #endregion


    #region Public Methods

    public static bool EnterDirectory(string dirName)
    {
        if (CurrentRootLoader is not null)
        {
            CurrentRootLoader.EnterDirectory(dirName);
            return true;
        }

        DirectoryLink.AddLast(dirName);

        if (DirectoryLink.First is { Value: "local" }) return false;

        CurrentRootLoader = RootMap.GetOrDefault(DirectoryLink.Join('/'))?.Invoke();
        return true;
    }

    public static void ExitDirectory()
    {
        if (DirectoryLink.Count == 0) return;

        if (CurrentRootLoader is null)
        {
            DirectoryLink.RemoveLast();
            return;
        }

        if (CurrentRootLoader.ExitDirectory())
        {
            DirectoryLink.RemoveLast();
            CurrentRootLoader = null;
        }
    }

    public static void LoadFile(string fileName, byte[] fileContent)
    {
        CurrentRootLoader?.LoadFile(fileName, fileContent);
    }

    #endregion
}