using System.Reflection;
using System.Reflection.Emit;

namespace BabelRushR.Core.Test;

/// <summary>在运行时造一个最小但真实可加载的程序集，供包加载相关的用例使用。</summary>
internal static class TestAssemblyBuilder
{
    public static byte[] BuildFixtureAssembly(string assemblyName)
    {
        var builder = new PersistedAssemblyBuilder(new AssemblyName(assemblyName), typeof(object).Assembly);
        var module = builder.DefineDynamicModule(assemblyName);
        var type = module.DefineType($"{assemblyName}.Marker", TypeAttributes.Public | TypeAttributes.Class);
        type.DefineDefaultConstructor(MethodAttributes.Public);
        type.CreateType();

        using var stream = new MemoryStream();
        builder.Save(stream);
        return stream.ToArray();
    }
}
