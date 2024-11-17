using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BabelRush.Generator.Generators;

[Generator(LanguageNames.CSharp)]
public class ModelCheckGenerator : IIncrementalGenerator
{
    private static class Names
    {
        public const string ModelAttribute = "BabelRush.Data.ModelAttribute";
        public const string NecessaryPropertyAttribute = "BabelRush.Data.NecessaryPropertyAttribute";

        public const string AllowNull = "System.Diagnostics.CodeAnalysis.AllowNull";
        public const string MaybeNull = "System.Diagnostics.CodeAnalysis.MaybeNull";
        public const string MaybeNullWhen = "System.Diagnostics.CodeAnalysis.MaybeNullWhen";

        public const string ListG = "System.Collections.Generic.List";

        public const string ModelDidNotInitializeException = "BabelRush.Data.ModelDidNotInitializeException";

        public const string TargetFileSuffix = "_ModelCheck.generated.cs";
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var infoProvider = context.SyntaxProvider
                                  .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                                  .Where(static x => x is not null)
                                  .Select(static (x, _) => x!.Value);

        context.RegisterSourceOutput(infoProvider, Execute);
    }


    #region Select Info

    private record struct ModelClassInfo(
        string? Namespace, string ClassName, string ClassFullName,
        IReadOnlyCollection<IPropertySymbol> NecessaryProperties
    );

    private static bool SyntaxPredicate(SyntaxNode s, CancellationToken _) =>
        s is ClassDeclarationSyntax { AttributeLists.Count: > 0 } @class
     && @class.Modifiers.All(modifier => !modifier.IsKind(SyntaxKind.StaticKeyword));

    private static ModelClassInfo? SyntaxTransform(GeneratorSyntaxContext c, CancellationToken _)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)c.Node;
        var model = c.SemanticModel;

        var classSymbol = model.GetDeclaredSymbol(classDeclarationSyntax)!;

        foreach (var attributeData in classSymbol.GetAttributes())
        {
            if (attributeData.AttributeClass.IsDerivedFrom(Names.ModelAttribute))
                return GetModelClassInfo(classSymbol);
        }

        return null;
    }

    private static ModelClassInfo GetModelClassInfo(INamedTypeSymbol classSymbol)
    {
        var nameSpace = classSymbol.ContainingNamespace?.ToDisplayString();
        var className = classSymbol.Name;
        var classFullName = classSymbol.ToDisplayString();
        var necessaryProperties =
            classSymbol.GetMembers()
                       .OfType<IPropertySymbol>()
                       .Where(static p =>
                                  !p.IsStatic
                               && p.GetAttributes().Any(static a => a.AttributeClass.IsDerivedFrom(Names.NecessaryPropertyAttribute))
                               && p is { GetMethod: not null, SetMethod: not null }
                             )
                       .ToImmutableArray();

        return new(nameSpace, className, classFullName, necessaryProperties);
    }

    #endregion


    private static void Execute(SourceProductionContext context, ModelClassInfo info)
    {
        IndentStringBuilder sourceBuilder = new();

        sourceBuilder.AppendLine($"namespace {info.Namespace};")
                     .AppendLine()
                     .AppendLine($"partial class {info.ClassName}")
                     .AppendLine("{");
        using (sourceBuilder.Indent())
        {
            sourceBuilder.AppendLine("//declare necessary properties");
            foreach (var property in info.NecessaryProperties)
            {
                string name = property.Name,
                       type = property.Type.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
                bool isRefType = property.Type.IsReferenceType;
                sourceBuilder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                             .AppendLine($"private bool _initialized_{name} = false;")
                             .AppendLine();

                if (isRefType) sourceBuilder.AppendLine($"[field: global::{Names.AllowNull}, global::{Names.MaybeNull}]");
                sourceBuilder.AppendLine($"public partial {type} {name}")
                             .AppendLine("{");
                using (sourceBuilder.Indent())
                {
                    sourceBuilder.AppendLine("get")
                                 .AppendLine("{")
                                 .IncreaseIndent()
                                 .AppendLine($"if (!_initialized_{name}) throw new global::{Names.ModelDidNotInitializeException}();")
                                 .AppendLine("return field;")
                                 .DecreaseIndent()
                                 .AppendLine("}");
                    sourceBuilder.AppendLine("set")
                                 .AppendLine("{")
                                 .IncreaseIndent()
                                 .AppendLine($"_initialized_{name} = true;")
                                 .AppendLine("field = value;")
                                 .DecreaseIndent()
                                 .AppendLine("}");
                }
                sourceBuilder.AppendLine("}")
                             .AppendLine();
            }

            sourceBuilder.AppendLine("//check necessary properties")
                         .AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                         .AppendLine("private bool Check(out string[] errors)")
                         .AppendLine("{");
            using (sourceBuilder.Indent())
            {
                sourceBuilder.AppendLine($"global::{Names.ListG}<string> errorList = [];");
                // bool first = true;
                foreach (var property in info.NecessaryProperties)
                {
                    var name = property.Name;
                    sourceBuilder.AppendLine($"if (!_initialized_{name}) errorList.Add(\"Property {name} did not initialized\");");
                }
                sourceBuilder.AppendLine()
                             .AppendLine("CustomCheck(errorList);");
                sourceBuilder.AppendLine()
                             .AppendLine("errors = errorList.ToArray();")
                             .AppendLine("return errorList.Count == 0;");
            }
            sourceBuilder.AppendLine("}");

            sourceBuilder.AppendLine("//custom check")
                         .AppendLine($"partial void CustomCheck(global::{Names.ListG}<string> errorList);");
        }
        sourceBuilder.AppendLine("}");

        context.AddSource($"{info.ClassFullName}{Names.TargetFileSuffix}", sourceBuilder.ToString());
    }
}