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
        // ReSharper disable UnusedMember.Local
        public const string ModelSetAttribute = "BabelRush.Data.ModelSetAttribute";

        public const string IModelSetG = "BabelRush.Data.IModelSet";

        
        public const string IEnumerable = "System.Collections.IEnumerable";
        
        public const string IEnumerator = "System.Collections.IEnumerator";
        public const string IEnumeratorG = "System.Collections.Generic.IEnumerator";
        public const string IReadOnlyCollectionG = "System.Collections.Generic.IReadOnlyCollection";
        public const string ListG = "System.Collections.Generic.List";

        public const string NameSpaceLinq = "System.Linq";

        public const string TargetFileSuffix = "_ModelSet.generated.cs";
        // ReSharper restore UnusedMember.Local
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
        sourceBuilder.AppendLine($"using {Names.NameSpaceLinq};")
                     .AppendLine()
                     .AppendLine($"namespace {info.Namespace};")
                     .AppendLine()
                     .AppendLine($"partial class {info.ClassName}")
                     .AppendLine("{");
        using (sourceBuilder.Indent())
        {
            sourceBuilder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                         .AppendLine($"private partial class ModelSet : {Names.IModelSetG}<{info.ClassName}>")
                         .AppendLine("{");
            using (sourceBuilder.Indent())
            {
                sourceBuilder.AppendLine($"public {Names.ListG}<{info.ClassName}> {info.SetName} {{ get; set; }} = [];");

                sourceBuilder.AppendLine()
                             .AppendLine($"public global::{Names.IReadOnlyCollectionG}<{info.ClassName}> CheckAll(out string[] errors)")
                             .AppendLine("{");
                using (sourceBuilder.Indent())
                {
                    sourceBuilder.AppendLine($"global::{Names.ListG}<{info.ClassName}> result = [];")
                                 .AppendLine($"global::{Names.ListG}<(string id, string[] messages)> errorList = [];")
                                  //.AppendLine($"foreach (var item in {info.SetName})")
                                 .AppendLine($"for (int i = 0; i < {info.SetName}.Count; i++)")
                                 .AppendLine("{");
                    using (sourceBuilder.Indent())
                    {
                        sourceBuilder.AppendLine($"var item = {info.SetName}[i];")
                                     .AppendLine("bool @checked = item.Check(out var error);")
                                     .AppendLine("errorList.Add((item.Id, error));");
                        sourceBuilder.AppendLine("if (@checked)")
                                     .AppendLine("{")
                                     .IncreaseIndent()
                                     .AppendLine("AfterCheck(ref item, errorList);")
                                     .AppendLine("result.Add(item);")
                                     .DecreaseIndent()
                                     .AppendLine("}");
                    }
                    sourceBuilder.AppendLine("}");
                    sourceBuilder.AppendLine("errors = errorList.SelectMany(t => t.messages, (t, m) => $\"in ID {t.id}: {m}\").ToArray();");
                    sourceBuilder.AppendLine("return result;");
                }
                sourceBuilder.AppendLine("}");

                sourceBuilder.AppendLine($"partial void AfterCheck(ref {info.ClassName} item, "
                                       + $"{Names.ListG}<(string id, string[] messages)> errorList);");
            }
            sourceBuilder.AppendLine("}");
        }
        sourceBuilder.AppendLine("}");

        context.AddSource($"{info.ClassFullName}{Names.TargetFileSuffix}", sourceBuilder.ToString());
    }
}