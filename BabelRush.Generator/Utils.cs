using Microsoft.CodeAnalysis;

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
}