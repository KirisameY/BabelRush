using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Registering.SourceTakers;

namespace BabelRush.Registering.RootLoaders;

public abstract class RootLoader<TSource> : IRootLoader
{
    protected abstract ISourceTaker<TSource>? GetSourceTaker(string path);
    public abstract bool EnterDirectory(string dirName);
    public abstract void LoadFile(string fileName, byte[] fileContent);
    public abstract bool ExitDirectory();
}

public class RootLoaderExitedException : Exception
{
    [StackTraceHidden]
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition)
    {
        if (condition) throw new RootLoaderExitedException();
    }
}