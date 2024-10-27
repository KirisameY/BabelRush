using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BabelRush.Generator;

public static class Utils
{
    public static bool IsDerivedFrom(this INamedTypeSymbol? type, string baseName)
    {
        while (type != null)
        {
            if (type.OriginalDefinition.ToDisplayString() == baseName) return true;
            type = type.BaseType;
        }
        return false;
    }
    
    public static string GetProjectRootNamespace(INamedTypeSymbol namedTypeSymbol)
    {
        var namespaceSymbol = namedTypeSymbol.ContainingNamespace;
        string rootNamespace = string.Empty;

        // 循环查找直到找到根命名空间
        while (namespaceSymbol != null && !namespaceSymbol.IsGlobalNamespace)
        {
            rootNamespace = namespaceSymbol.ToDisplayString();
            namespaceSymbol = namespaceSymbol.ContainingNamespace;
        }

        // 返回最后找到的命名空间（即根命名空间）
        return rootNamespace;
    }
}