using System.Reflection;
using System.Runtime.Loader;

using BabelRushR.Core.Common;

namespace BabelRushR.Core.Test;

public class PackAssemblyLoadContextTests
{
    [Fact]
    public void Load_Resolves_An_Assembly_From_A_Dependency_Context()
    {
        const string assemblyName = "Fixture.DependencyAssembly";
        var dependencyContext = new PackAssemblyLoadContext("dependency", []);
        var dependency = dependencyContext.LoadFromStream(new MemoryStream(TestAssemblyBuilder.BuildFixtureAssembly(assemblyName)));
        var dependentContext = new PackAssemblyLoadContext("dependent", [dependencyContext]);

        var resolved = dependentContext.LoadFromAssemblyName(new AssemblyName(assemblyName));

        // 自己的上下文的 base.Load 找不到时，必须往依赖包（加载顺序更早的包）的上下文里找。
        Assert.Same(dependency, resolved);
    }

    [Fact]
    public void Load_Throws_When_Neither_The_Context_Nor_Its_Dependencies_Can_Resolve()
    {
        var context = new PackAssemblyLoadContext("empty", []);

        Assert.Throws<FileNotFoundException>(() =>
                                                 context.LoadFromAssemblyName(new AssemblyName("Fixture.NotInstalled")));
    }
}
