using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BabelRush.Generator.Generators;

[Generator(LanguageNames.CSharp)]
public class ModelCheckGenerator : IIncrementalGenerator //todo:继承关系
{
    private static class Names
    {
        public const string ModelAttribute = "BabelRush.Data.ModelAttribute";
        public const string NecessaryPropertyAttribute = "BabelRush.Data.NecessaryPropertyAttribute";

        public const string AllowNull = "System.Diagnostics.CodeAnalysis.AllowNull";
        public const string MaybeNull = "System.Diagnostics.CodeAnalysis.MaybeNull";

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
        bool Inherited, bool Sealed,
        IReadOnlyCollection<IPropertySymbol> ModelProperties, IReadOnlyCollection<IPropertySymbol> NecessaryProperties
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
        var inherited = classSymbol.BaseType is { } baseType &&
            baseType.GetAttributes().Any(a => a.AttributeClass.IsDerivedFrom(Names.ModelAttribute));
        var @sealed = classSymbol.IsSealed;
        var properties =
            classSymbol.GetMembers()
                       .OfType<IPropertySymbol>()
                       .Where(static p => !p.IsStatic && p is { GetMethod: not null, SetMethod: not null });
        List<IPropertySymbol> modelProperties = [],
                              necessaryProperties = [];
        foreach (var property in properties)
        {
            if (property.Type.GetAttributes().Any(static a => a.AttributeClass.IsDerivedFrom(Names.ModelAttribute)))
                modelProperties.Add(property);
            if (property.GetAttributes().Any(static a => a.AttributeClass.IsDerivedFrom(Names.NecessaryPropertyAttribute)))
                necessaryProperties.Add(property);
        }

        return new(nameSpace, className, classFullName, inherited, @sealed, modelProperties, necessaryProperties);
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
                                 .AppendLine("#pragma warning disable CS0472 // The result of the expression is always 'false' since a null reference is passed for parameter 'value'")
                                 .AppendLine("if (value == null) return;")
                                 .AppendLine("#pragma warning restore CS0472")
                                 .AppendLine()
                                 .AppendLine($"_initialized_{name} = true;")
                                 .AppendLine("field = value;")
                                 .DecreaseIndent()
                                 .AppendLine("}");
                }
                sourceBuilder.AppendLine("}")
                             .AppendLine();
            }

            sourceBuilder.AppendLine("//check properties")
                         .AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                         .Append("public " + (info.Inherited, info.Sealed) switch
                          {
                              (true, _)      => "override ",
                              (false, false) => "virtual ",
                              (false, true)  => ""
                          })
                         .AppendLine("bool Check(out string[] errors)")
                         .AppendLine("{");
            using (sourceBuilder.Indent())
            {
                sourceBuilder.AppendLine($"global::{Names.ListG}<string> errorList = [];")
                             .AppendLine();

                if (info.Inherited)
                {
                    sourceBuilder.AppendLine("_ = base.Check(out var baseErrors);")
                                 .AppendLine("errorList.AddRange(baseErrors);");
                }
                // bool first = true;
                foreach (var property in info.NecessaryProperties)
                {
                    var name = property.Name;
                    sourceBuilder.AppendLine($"if (!_initialized_{name}) errorList.Add(\"Property {name} of {info.ClassName} did not initialized\");");
                }

                sourceBuilder.AppendLine()
                             .AppendLine("#pragma warning disable CS0168 // variable is declared but not used")
                             .AppendLine("string[] errorsArray;")
                             .AppendLine("#pragma warning restore CS0168");
                foreach (var property in info.ModelProperties)
                {
                    var name = property.Name;
                    sourceBuilder.AppendLine($"if (!{name}.Check(out errorsArray)) errorList.AddRange(errorsArray);");
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