using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BabelRush.Generator.Generators;

[Generator(LanguageNames.CSharp)]
public class ModelSetGenerator : IIncrementalGenerator
{
    private static class Names
    {
        // ReSharper disable InconsistentNaming
        public const string ModelSetAttribute = "BabelRush.Data.ModelSetAttribute";

        public const string IEnumerable = "System.Collections.IEnumerable";
        public const string IEnumerator = "System.Collections.IEnumerator";
        public const string IEnumeratorG = "System.Collections.Generic.IEnumerator";
        public const string IReadOnlyCollectionG = "System.Collections.Generic.IReadOnlyCollection";
        public const string ListG = "System.Collections.Generic.List";
        // ReSharper restore InconsistentNaming
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //get classes
        var infoProvider = context.SyntaxProvider
                                  .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                                  .Where(static x => x is not null)
                                  .Select(static (x, _) => x!.Value);

        //generate partial class
        context.RegisterSourceOutput(infoProvider, Execute);
    }


    #region Select Info

    private record struct ModelSetInfo(string? Namespace, string ClassName, string ClassFullName, string SetName);

    private static bool SyntaxPredicate(SyntaxNode s, CancellationToken _) =>
        s is ClassDeclarationSyntax { AttributeLists.Count: > 0 } @class
     && @class.Modifiers.All(modifier => !modifier.IsKind(SyntaxKind.StaticKeyword));

    private static ModelSetInfo? SyntaxTransform(GeneratorSyntaxContext c, CancellationToken _)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)c.Node;
        var model = c.SemanticModel;

        var classSymbol = model.GetDeclaredSymbol(classDeclarationSyntax)!;

        foreach (var attributeData in classSymbol.GetAttributes())
        {
            if (!attributeData.AttributeClass.IsDerivedFrom(Names.ModelSetAttribute)) continue;

            var nameSpace = classSymbol.ContainingNamespace?.ToDisplayString();
            var className = classSymbol.Name;
            var classFullName = classSymbol.ToDisplayString();
            var setName = (string)attributeData.ConstructorArguments[0].Value!;

            return new(nameSpace, className, classFullName, setName);
        }

        return null;
    }

    #endregion


    private static void Execute(SourceProductionContext context, ModelSetInfo info)
    {
        IndentStringBuilder sourceBuilder = new();
        sourceBuilder.AppendLine($"namespace {info.Namespace};")
                     .AppendLine()
                     .AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                     .AppendLine($"public partial class {info.ClassName}")
                     .AppendLine("{");
        using (sourceBuilder.Indent())
        {
            sourceBuilder.AppendLine($"private class ModelSet : global::{Names.IReadOnlyCollectionG}<{info.ClassName}>")
                         .AppendLine("{");
            using (sourceBuilder.Indent())
            {
                sourceBuilder.AppendLine($"public {Names.ListG}<{info.ClassName}> {info.SetName} {{ get; set; }} = [];")
                             .AppendLine()
                             .AppendLine($"public {Names.IEnumeratorG}<{info.ClassName}> GetEnumerator() => {info.SetName}.GetEnumerator();")
                             .AppendLine($"{Names.IEnumerator} {Names.IEnumerable}.GetEnumerator() => {info.SetName}.GetEnumerator();")
                             .AppendLine($"public int Count => {info.SetName}.Count;");
            }
            sourceBuilder.AppendLine("}");
        }
        sourceBuilder.AppendLine("}");

        context.AddSource($"{info.ClassFullName}_ModelSet.generated.cs", sourceBuilder.ToString());
    }
}