using System.Collections.Immutable;
using System.Reflection;
using System.Runtime.Loader;

namespace BabelRushR.Core.Common;

public class PackAssemblyLoadContext(string packName, ImmutableArray<AssemblyLoadContext> dependencies) : AssemblyLoadContext($"PackAsmCtx({packName})")
{
    private readonly Lock _lock = new();
    private readonly HashSet<string> _failedCache = [];

    protected override Assembly? Load(AssemblyName assemblyName)
    {
        var fullName = assemblyName.FullName;
        using var lockHandle = _lock.EnterScope();
        if (_failedCache.Contains(fullName)) return null;

        // If one day they finally implemented AssemblyLoadContext.TryLoadFromAssemblyName, we can use it, but now...
        var result = base.Load(assemblyName) ?? dependencies.Select(ctx =>
        {
            try
            {
                return ctx.LoadFromAssemblyName(assemblyName);
            }
            catch (IOException)
            {
                return null;
            }
        }).FirstOrDefault(a => a is not null);

        if (result is null) _failedCache.Add(fullName);
        return result;
    }
}