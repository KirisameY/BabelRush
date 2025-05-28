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
        // ReSharper disable MemberCanBePrivate.Local
        public const string ModelSetAttribute = "BabelRush.Data.ModelSetAttribute";
        public const string ModelSetAttributeG = "BabelRush.Data.ModelSetAttribute";
        public const string ModelSetAttributeGT = $"{ModelSetAttributeG}<TModel>";

        public const string IModelSetG = "BabelRush.Data.IModelSet";


        public const string IEnumerable = "System.Collections.IEnumerable";

        public const string IEnumerator = "System.Collections.IEnumerator";
        public const string IEnumeratorG = "System.Collections.Generic.IEnumerator";
        public const string IReadOnlyCollectionG = "System.Collections.Generic.IReadOnlyCollection";
        public const string ListG = "System.Collections.Generic.List";

        public const string NameSpaceLinq = "System.Linq";

        public const string TargetFileSuffix = "_ModelSet.generated.cs";
        // ReSharper restore MemberCanBePrivate.Local
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

    private record struct ModelSetInfo(string? Namespace, string ClassName, string ClassFullName, List<(string Name, string Type)> Sets);

    private static bool SyntaxPredicate(SyntaxNode s, CancellationToken _) =>
        s is ClassDeclarationSyntax { AttributeLists.Count: > 0 } @class
     && @class.Modifiers.All(modifier => !modifier.IsKind(SyntaxKind.StaticKeyword));

    private static ModelSetInfo? SyntaxTransform(GeneratorSyntaxContext c, CancellationToken _)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)c.Node;
        var model = c.SemanticModel;

        var classSymbol = model.GetDeclaredSymbol(classDeclarationSyntax)!;

        List<(string Name, string Type)> sets = [];

        foreach (var attributeData in classSymbol.GetAttributes())
        {
            string setType;

            if (attributeData.AttributeClass.IsDerivedFrom(Names.ModelSetAttribute))
                setType = classSymbol.ToDisplayString();
            else if (attributeData.AttributeClass.IsDerivedFrom(Names.ModelSetAttributeGT))
                setType = attributeData.AttributeClass!.TypeArguments[0].ToDisplayString();
            else continue;

            var setName = (string)attributeData.ConstructorArguments[0].Value!;

            sets.Add((setName, setType));
        }

        if (sets.Count == 0) return null;

        var nameSpace = classSymbol.ContainingNamespace?.ToDisplayString();
        var className = classSymbol.Name;
        var classFullName = classSymbol.ToDisplayString();
        return new(nameSpace, className, classFullName, sets);
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
                foreach (var set in info.Sets)
                {
                    sourceBuilder.AppendLine($"public global::{Names.ListG}<global::{set.Type}> {set.Name} {{ get; set; }} = [];");
                }

                sourceBuilder.AppendLine()
                             .AppendLine($"public global::{Names.IReadOnlyCollectionG}<{info.ClassName}> CheckAll(out string[] errors)")
                             .AppendLine("{");
                using (sourceBuilder.Indent())
                {
                    sourceBuilder.AppendLine($"global::{Names.ListG}<{info.ClassName}> result = [];")
                                 .AppendLine($"global::{Names.ListG}<(string id, string[] messages)> errorList = [];")
                                 .AppendLine();
                    sourceBuilder.Append($"var sets = {info.Sets[0].Name}");
                    foreach (var set in info.Sets.Skip(1))
                    {
                        sourceBuilder.AppendLine()
                                     .Append($"          .Concat({set.Name})");
                    }
                    sourceBuilder.AppendLine(";");


                    sourceBuilder.AppendLine("foreach (var item in sets)")
                                 .AppendLine("{");
                    using (sourceBuilder.Indent())
                    {
                        sourceBuilder.AppendLine("bool @checked = item.Check(out var error);")
                                     .AppendLine("errorList.Add((item.Id, error));");
                        sourceBuilder.AppendLine("if (@checked)")
                                     .AppendLine("{")
                                     .IncreaseIndent()
                                     .AppendLine($"{info.ClassName} refItem = item;")
                                     .AppendLine("AfterCheck(ref refItem, errorList);")
                                     .AppendLine("result.Add(refItem);")
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