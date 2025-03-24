using System.Collections.Generic;

using BabelRush.Registering.RootLoaders;

namespace BabelRush.Registering.FileLoading;

internal abstract class FileLoader
{
    private LinkedList<string> DirectoryLink { get; } = [];
    private IRootLoader? CurrentRootLoader { get; set; }

    /// <returns>If should enter this directory</returns>
    protected abstract bool EnterRootDirectory(LinkedList<string> directoryLink, out IRootLoader? rootLoader);

    /// <returns>If should enter this directory</returns>
    public bool EnterDirectory(string dirName)
    {
        if (CurrentRootLoader is not null)
        {
            CurrentRootLoader.EnterDirectory(dirName);
            return true;
        }

        DirectoryLink.AddLast(dirName);

        var result = EnterRootDirectory(DirectoryLink, out var rootLoader);
        CurrentRootLoader = rootLoader;
        return result;
    }

    public bool ExitDirectory()
    {
        if (DirectoryLink.Count == 0) return true;

        if (CurrentRootLoader is null)
        {
            DirectoryLink.RemoveLast();
            return false;
        }

        if (CurrentRootLoader.ExitDirectory())
        {
            DirectoryLink.RemoveLast();
            CurrentRootLoader = null;
        }

        return false;
    }

    public void LoadFile(string fileName, byte[] fileContent)
    {
        CurrentRootLoader?.LoadFile(fileName, fileContent);
    }
}