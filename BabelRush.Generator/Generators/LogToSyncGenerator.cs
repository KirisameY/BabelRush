using System.Diagnostics.CodeAnalysis;

using KirisameLib.GeneratorTools;
using KirisameLib.GeneratorTools.Extensions;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BabelRush.Generator.Generators;

[Generator(LanguageNames.CSharp)]
public class LogToSyncGenerator : IIncrementalGenerator
{
    private static class Names
    {
        public const string LogToSyncAttribute = "BabelRush.Utils.LogToSyncAttribute";

        public const string TargetFileSuffix = "_LogToSync.generated.cs";
    }

    [SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2000:Add analyzer diagnostic IDs to analyzer release")]
    [SuppressMessage("MicrosoftCodeAnalysisReleaseTracking", "RS2008:Enable analyzer release tracking for the analyzer project containing rule")]
    private static class Diagnostics
    {
        public static readonly DiagnosticDescriptor MethodNameNotEndWithAsync = new(
            id: "BABELRUSH_LTS001",
            title: "Method name is not end with 'Async'",
            messageFormat: "The method name '{0}' is not end with 'Async'",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Method with LogToSyncAttribute should have a name end with 'Async', otherwise generator will not work."
        );
        public static readonly DiagnosticDescriptor MethodNotAsync = new(
            id: "BABELRUSH_LTS002",
            title: "Method is not async or abstract",
            messageFormat: "The method '{0}' is not async or abstract",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Method with LogToSyncAttribute should have a name end with 'Async', otherwise generator will not work."
        );
        public static readonly DiagnosticDescriptor ClassNotHaveLogger = new(
            id: "BABELRUSH_LTS003",
            title: "Class do not have a Logger",
            messageFormat: "The class '{0}' do not have a 'Logger' property",
            category: "Usage",
            defaultSeverity: DiagnosticSeverity.Warning,
            isEnabledByDefault: true,
            description: "Class that contains any method with LogToSyncAttribute should have a static property named 'Logger', otherwise generator will not work."
        );
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var infoProvider = context.SyntaxProvider
                                  .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                                  .WhereNotNull()
                                  .Select((info, _) => info!.Value);

        context.RegisterSourceOutput(infoProvider, Execute);
    }


    private record struct ModelClassInfo(
        string? Namespace, string ClassName, string ClassFullName,
        INamedTypeSymbol ClassSymbol, IEnumerable<IMethodSymbol> MethodSymbols
    );

    #region Select Info

    private static bool SyntaxPredicate(SyntaxNode s, CancellationToken cancelToken)
    {
        if (s is not ClassDeclarationSyntax { Members.Count: > 0 } cls) return false;
        return cls.Modifiers.Any(mo => mo.IsKind(SyntaxKind.PublicKeyword) || mo.IsKind(SyntaxKind.ProtectedKeyword)
                                  || mo.IsKind(SyntaxKind.InternalKeyword) || mo.IsKind(SyntaxKind.PrivateKeyword)) &&
        (
            cls.Modifiers.Any(mo => mo.IsKind(SyntaxKind.AbstractKeyword)) ||
            cls.Members.Any(m => m is MethodDeclarationSyntax { AttributeLists.Count: > 0 }
                             && m.Modifiers.Any(mo => mo.IsKind(SyntaxKind.AsyncKeyword)))
        );
    }

    private static ModelClassInfo? SyntaxTransform(GeneratorSyntaxContext c, CancellationToken cancelToken)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)c.Node;
        var model = c.SemanticModel;

        var classSymbol = model.GetDeclaredSymbol(classDeclarationSyntax)!;

        List<IMethodSymbol> methods = [];
        foreach (var methodSymbol in classSymbol.GetMembers().OfType<IMethodSymbol>())
        {
            if (cancelToken.IsCancellationRequested) throw new OperationCanceledException(cancelToken);
            if (methodSymbol.GetAttributes().Any(att => att.AttributeClass.IsDerivedFrom(Names.LogToSyncAttribute)))
                methods.Add(methodSymbol);
        }

        if (methods.Count == 0) return null;
        return GetModelClassInfo(classSymbol, methods);
    }

    private static ModelClassInfo GetModelClassInfo(INamedTypeSymbol classSymbol, IEnumerable<IMethodSymbol> methods)
    {
        var ns = classSymbol.ContainingNamespace?.ToDisplayString();
        var className = classSymbol.Name;
        var classFullName = classSymbol.ToDisplayString();

        return new(ns, className, classFullName, classSymbol, methods);
    }

    #endregion


    private static void Execute(SourceProductionContext context, ModelClassInfo info)
    {
        IndentStringBuilder sourceBuilder = new();
        if (info.ClassSymbol.GetMembers().OfType<IPropertySymbol>().All(s => s.Name != "Logger" || !s.IsStatic))
        {
            var location = info.ClassSymbol.Locations.FirstOrDefault();
            var diagnostic = Diagnostic.Create(Diagnostics.ClassNotHaveLogger, location, info.ClassSymbol.Name);
            context.ReportDiagnostic(diagnostic);
            return;
        }

        sourceBuilder.AppendLine("#nullable enable")
                     .AppendLine($"namespace {info.Namespace};")
                     .AppendLine()
                     .AppendLine($"partial class {info.ClassName}");
        using (sourceBuilder.IndentWithBrace())
        {
            foreach (var method in info.MethodSymbols)
            {
                if (!method.Name.EndsWith("Async"))
                {
                    var location = method.Locations.FirstOrDefault();
                    var diagnostic = Diagnostic.Create(Diagnostics.MethodNameNotEndWithAsync, location, method.Name);
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }
                if (method is { IsAsync: false, IsAbstract: false })
                {
                    var location = method.Locations.FirstOrDefault();
                    var diagnostic = Diagnostic.Create(Diagnostics.MethodNotAsync, location, method.Name);
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }

                var accessibility = method.DeclaredAccessibility.ToDefinitionString();
                var prefix = $"{accessibility}{(method.IsStatic ? "static " : "")}";
                var name = method.Name[..^5];

                var typeParas = method.TypeParameters
                                      .Select(t => t.Name)
                                      .Join(", ");
                if (typeParas is not "") typeParas = $"<{typeParas}>";
                var paras = method.Parameters
                                  .Select(ParameterSymbolExtensions.ToDefinitionString)
                                  .Join(", ");
                var paraNames = method.Parameters
                                      .Select(p => $"@{p.Name}")
                                      .Join(", ");

                sourceBuilder.AppendLine(Project.GeneratedCodeAttribute)
                             .AppendLine($"{prefix}void {name}{typeParas}({paras})")
                             .IncreaseIndent();
                method.TypeParameters.SelectNotNull(t => t.GetConstraintString())
                      .ForEach(c => sourceBuilder.AppendLine(c));
                sourceBuilder.AppendLine($"=> {method.Name}{typeParas}({paraNames}).ContinueWith(t =>")
                             .DecreaseIndent();
                using (sourceBuilder.IndentWith("{", "});"))
                {
                    sourceBuilder.AppendLine("if (!t.IsFaulted) return;")
                                 .AppendLine($"""Logger.Log(global::KirisameLib.Logging.LogLevel.Error, "{name}", """
                                           + """$"Exception thrown: {t.Exception?.Flatten()}");""")
                                 .AppendLine($"""Logger.Log(global::KirisameLib.Logging.LogLevel.Debug, "{name}", """
                                           + """$"StackTrace: {t.Exception?.StackTrace}");""");
                }
            }
        }

        context.AddSource($"{info.ClassFullName}{Names.TargetFileSuffix}", sourceBuilder.ToString());
    }
}