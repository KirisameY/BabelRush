using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

using BabelRush.Registering.SourceTakers;

namespace BabelRush.Registering.RootLoaders;

public abstract class RootLoader
{
    public abstract bool EnterDirectory(string dirName);
    public abstract void LoadFile(string fileName, byte[] fileContent);

    /// <returns>true if the root directory was exited</returns>
    public abstract bool ExitDirectory();
}

public abstract class RootLoader<TSource, TSourceTaker> : RootLoader
    where TSourceTaker : ISourceTaker<TSource>
{
    private Dictionary<string, TSourceTaker> PathMap { get; } = new();

    [field: AllowNull, MaybeNull]
    protected ReadOnlyDictionary<string, TSourceTaker> PathMapView => field ??= PathMap.AsReadOnly();

    public bool AddRegistrant(string path, TSourceTaker registrant) => PathMap.TryAdd(path, registrant);
}

public class RootLoaderExitedException : Exception
{
    [StackTraceHidden]
    public static void ThrowIf([DoesNotReturnIf(true)] bool condition)
    {
        if (condition) throw new RootLoaderExitedException();
    }
}