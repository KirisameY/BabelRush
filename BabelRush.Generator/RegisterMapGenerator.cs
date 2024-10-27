using System.Collections.Immutable;
using System.Text;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BabelRush.Generator;

[Generator(LanguageNames.CSharp)]
public class RegisterMapGenerator : IIncrementalGenerator
{
    private static class Names
    {
        public const string RegisterContainerAttribute = "BabelRush.Registering.RegisterContainerAttribute";
        public const string RegisterAttribute = "BabelRush.Registering.RegisterAttribute";

        public const string TargetFile = "BabelRush.RegisterMap.g.cs";
        public const string GenerateNamespace = "_Generated.BabelRush";
        public const string GeneratedClass = "RegisterMap";
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var classProvider = GetTargetClassProvider(context);

        context.RegisterSourceOutput(classProvider, Execute);
    }

    private record struct RegisterMapInfo(string RootNameSpace) { }

    private static IncrementalValueProvider<RegisterMapInfo> GetTargetClassProvider(
        IncrementalGeneratorInitializationContext context)
    {
        return context.SyntaxProvider
                      .CreateSyntaxProvider(Predicate, Transform)
                      .Where(static x => x is not null)!
                      .Collect()
                      .Select(Select);


        static bool Predicate(SyntaxNode s, CancellationToken _) =>
            s is ClassDeclarationSyntax { AttributeLists.Count: > 0 } @class
         && @class.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword));

        static INamedTypeSymbol? Transform(GeneratorSyntaxContext c, CancellationToken _)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)c.Node;
            var model = c.SemanticModel;

            var typeDeclaredSymbol = model.GetDeclaredSymbol(classDeclarationSyntax)!;

            foreach (var attributeData in typeDeclaredSymbol.GetAttributes())
            {
                if (attributeData.AttributeClass.IsDerivedFrom(Names.RegisterContainerAttribute)) return typeDeclaredSymbol;
            }

            return null;
        }

        static RegisterMapInfo Select(ImmutableArray<INamedTypeSymbol?> s, CancellationToken _)
        {
            return new RegisterMapInfo(Utils.GetProjectRootNamespace(s.First()!));
        }
    }

    private static void Execute(SourceProductionContext context, RegisterMapInfo info)
    {
        //initialize
        IndentStringBuilder sourceBuilder = new();

        //select
        // foreach (var classDeclaration in classDeclarationSyntax)
        // {
        //     foreach (var propertyDeclaration in classDeclaration.Members.OfType<PropertyDeclarationSyntax>()) { }
        // }

        //generate
        sourceBuilder.AppendLine("//Generated RegisterMap Class:")
                     .AppendLine($"namespace {info.RootNameSpace}.{Names.GenerateNamespace};")
                     .AppendLine("");
        sourceBuilder.AppendLine($"[System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                     .AppendLine($"public static class {Names.GeneratedClass}")
                     .AppendLine("{");

        using (sourceBuilder.Indent())
        {
            sourceBuilder.AppendLine("public static void Register()")
                         .AppendLine("{");
            using (sourceBuilder.Indent())
            {
                sourceBuilder.AppendLine("//BabelRush.Registering.RegisterMap.Register();");
                //todo:把信息筛出来往这儿写
            }
            sourceBuilder.AppendLine("}");
        }

        sourceBuilder.AppendLine("}");


        //add to source
        context.AddSource(Names.TargetFile, sourceBuilder.ToString());
    }
}