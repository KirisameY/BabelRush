using System.Collections.Generic;

using BabelRush.Registering.RootLoaders;

namespace BabelRush.Registering.FileLoading;

internal abstract class FileLoader
{
    private LinkedList<string> DirectoryLink { get; } = [];
    private RootLoader? CurrentRootLoader { get; set; }

    protected abstract bool EnterRootDirectory(LinkedList<string> directoryLink, out RootLoader? rootLoader);

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

    public void ExitDirectory()
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

    public void LoadFile(string fileName, byte[] fileContent)
    {
        CurrentRootLoader?.LoadFile(fileName, fileContent);
    }
}