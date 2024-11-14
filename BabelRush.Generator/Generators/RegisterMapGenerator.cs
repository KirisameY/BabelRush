using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BabelRush.Generator.Generators;

[Generator(LanguageNames.CSharp)]
public class RegisterMapGenerator : IIncrementalGenerator
{
    private static class Names
    {
        // ReSharper disable InconsistentNaming
        // ReSharper disable UnusedMember.Local
        public const string RegisterContainerAttribute = "BabelRush.Registering.RegisterContainerAttribute";
        public const string RegisterAttribute = "BabelRush.Registering.RegisterAttribute";

        public const string LangRegisterAttribute = "BabelRush.Registering.LangRegisterAttribute<TModel, TTarget>";
        public const string ResRegisterAttribute = "BabelRush.Registering.ResRegisterAttribute<TModel, TTarget>";
        public const string DataRegisterAttribute = "BabelRush.Registering.DataRegisterAttribute<TModel, TTarget>";

        public const string IRegister = "KirisameLib.Data.Register.IRegister<T>";
        public const string IRegisterPure = "KirisameLib.Data.Register.IRegister";

        public const string CommonRegister = "KirisameLib.Data.Register.CommonRegister<T>";
        public const string CommonRegisterPure = "KirisameLib.Data.Register.CommonRegister";
        public const string LocalizedRegister = "KirisameLib.Data.I18n.LocalizedRegister<T>";
        public const string LocalizedRegisterPure = "KirisameLib.Data.I18n.LocalizedRegister";

        public const string DataRegistrant = "BabelRush.Registering.DataRegistrant";
        public const string ResRegistrant = "BabelRush.Registering.ResRegistrant";
        public const string LangRegistrant = "BabelRush.Registering.LangRegistrant";

        public const string AddDataRegistrant = "BabelRush.Registering.FileLoader.AddDataRegistrant";
        public const string AddDefaultResRegistrant = "BabelRush.Registering.FileLoader.AddDefaultResRegistrant";
        public const string AddLocalResRegistrant = "BabelRush.Registering.FileLoader.AddLocalResRegistrant";
        public const string AddLangRegistrant = "BabelRush.Registering.FileLoader.AddLangRegistrant";


        public const string PartialFileSuffix = "_RegisterMap.generated.cs";

        public const string TargetFile = "BabelRush.RegisterMap.generated.cs";
        public const string GenerateNamespace = "BabelRushGenerated";
        public const string GeneratedClass = "RegisterMap";
        // ReSharper restore UnusedMember.Local
        // ReSharper restore InconsistentNaming
    }


    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        //get classes
        var classSymbolsProvider = context.SyntaxProvider
                                          .CreateSyntaxProvider(SyntaxPredicate, SyntaxTransform)
                                          .Where(static x => x is not null)
                                          .Select(static (x, _) => x!);

        //check source class
        //todo:加入代码检查器，不合法的情况报错，不建议的情况警告
        var invalidClassedProvider = classSymbolsProvider;
        var checkedClassesProvider = classSymbolsProvider;

        //generate partial class
        //todo:.net9发布了之后，要改成让用户定义公共接口，脚本生成file的支持字段
        var partialExecutionProvider = checkedClassesProvider
           .Select(GetRegisterContainerInfo);
        context.RegisterSourceOutput(partialExecutionProvider, PartialExecute);

        //generate global class
        var globalExecutionProvider = context.CompilationProvider
                                             .Combine(checkedClassesProvider.Collect());
        context.RegisterSourceOutput(globalExecutionProvider, (c, info) => GlobalExecute(c, info.Left, info.Right));
    }


    #region Select Classes

    private static bool SyntaxPredicate(SyntaxNode s, CancellationToken _) =>
        s is ClassDeclarationSyntax { AttributeLists.Count: > 0 } @class
     && @class.Modifiers.Any(modifier => modifier.IsKind(SyntaxKind.StaticKeyword));

    private static INamedTypeSymbol? SyntaxTransform(GeneratorSyntaxContext c, CancellationToken _)
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

    #endregion


    #region Partial Generation

    private record struct RegisterInfo(IFieldSymbol Register, AttributeData Attribute);

    private record struct RegisterContainerInfo(string? NameSpace, string ClassName, ImmutableArray<RegisterInfo> RegisterInfos);

    private static RegisterContainerInfo GetRegisterContainerInfo(INamedTypeSymbol classSymbol, CancellationToken _)
    {
        string? nameSpace = classSymbol.ContainingNamespace?.ToDisplayString();
        string className = classSymbol.Name;
        var registerInfos =
            from field in classSymbol.GetMembers().OfType<IFieldSymbol>()
            let attribute = field.GetAttributes()
                                 .FirstOrDefault(static a => a.AttributeClass.IsDerivedFrom(Names.RegisterAttribute))
            where attribute is not null
            select new RegisterInfo(field, attribute);

        return new(nameSpace, className, registerInfos.ToImmutableArray());
    }

    private static void PartialExecute(SourceProductionContext context, RegisterContainerInfo info)
    {
        //check
        List<RegisterInfo> checkedInfos = [];
        foreach (var registerInfo in info.RegisterInfos)
        {
            var register = registerInfo.Register;
            var attribute = registerInfo.Attribute;

            if (!register.Name.EndsWith("Register"))
            {
                //todo:报错
                continue;
            }
            //.net9发了就能改了
            switch (attribute.AttributeClass!.OriginalDefinition.ToDisplayString())
            {
                case Names.DataRegisterAttribute or Names.ResRegisterAttribute:
                    if (register.Type.OriginalDefinition.ToDisplayString() != Names.CommonRegister)
                    {
                        //todo:报错
                        continue;
                    }
                    break;
                case Names.LangRegisterAttribute:
                    if (register.Type.OriginalDefinition.ToDisplayString() != Names.LocalizedRegister)
                    {
                        //todo:报错
                        continue;
                    }
                    break;
            }

            if (!SymbolEqualityComparer.Default.Equals(attribute.AttributeClass!.TypeArguments[1],
                                                       (register.Type as INamedTypeSymbol)!.TypeArguments[0]))
            {
                //todo:报错
                continue;
            }

            checkedInfos.Add(registerInfo);
        }

        //generate
        var sourceBuilder = new IndentStringBuilder();

        sourceBuilder.AppendLine($"namespace {info.NameSpace};")
                     .AppendLine("");
        sourceBuilder.AppendLine($"static partial class {info.ClassName}")
                     .AppendLine("{");
        using (sourceBuilder.Indent())
        {
            foreach (var registerInfo in checkedInfos)
            {
                var register = registerInfo.Register;
                var attribute = registerInfo.Attribute;
                var registerName = register.Name.Remove(register.Name.Length - "Register".Length);
                ITypeSymbol targetType = attribute.AttributeClass!.TypeArguments[1];

                sourceBuilder.AppendLine($"//{register.Name}");

                if (attribute.AttributeClass!.OriginalDefinition.ToDisplayString() == Names.ResRegisterAttribute)
                {
                    sourceBuilder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                                 .AppendLine($"private static global::{Names.LocalizedRegisterPure}<{targetType}> "
                                           + $"{registerName}LocalizedRegister = new(\"{registerName}LocalizedRegister\", {register.Name});");
                    sourceBuilder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                                 .AppendLine($"public static global::{Names.IRegisterPure}<{targetType}> "
                                           + $"{registerName} => {registerName}LocalizedRegister;");
                }
                else
                {
                    sourceBuilder.AppendLine($"public static {Names.IRegisterPure}<{targetType}> "
                                           + $"{registerName} => {register.Name};");
                }
            }

            sourceBuilder.AppendLine("")
                         .AppendLine("//register method");
            sourceBuilder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                         .AppendLine("internal static void Register()")
                         .AppendLine("{");
            using (sourceBuilder.Indent())
            {
                //遍历注册表信息，生成向文件加载器添加注册器的方法
                foreach (var registerInfo in checkedInfos)
                {
                    var register = registerInfo.Register;
                    var attribute = registerInfo.Attribute;
                    var registerName = register.Name.Remove(register.Name.Length - "Register".Length);
                    var genericTypes = attribute.AttributeClass!.TypeArguments;
                    var args = attribute.ConstructorArguments;

                    sourceBuilder.AppendLine($"//{register.Name}");
                    switch (attribute.AttributeClass!.OriginalDefinition.ToDisplayString())
                    {
                        case Names.DataRegisterAttribute:
                            var waitFor = $", {string.Join(", ", args[1].Values.Select(s => $"\"{s.Value}\""))}";
                            if (waitFor == ", ") waitFor = "";
                            sourceBuilder.AppendLine($"global::{Names.AddDataRegistrant}(\"{args[0].Value}\", "
                                                   + $"global::{Names.DataRegistrant}.Get<{genericTypes[0]}, {genericTypes[1]}>"
                                                   + $"({register.Name}{waitFor}));");
                            break;
                        case Names.ResRegisterAttribute:
                            sourceBuilder.AppendLine($"global::{Names.AddDefaultResRegistrant}(\"{args[0].Value}\", "
                                                   + $"global::{Names.ResRegistrant}.Get<{genericTypes[0]}, {genericTypes[1]}>"
                                                   + $"({register.Name}));")
                                         .AppendLine($"global::{Names.AddDefaultResRegistrant}(\"{args[0].Value}\", "
                                                   + $"global::{Names.ResRegistrant}.Get<{genericTypes[0]}, {genericTypes[1]}>"
                                                   + $"({registerName}LocalizedRegister));");
                            break;
                        case Names.LangRegisterAttribute:
                            sourceBuilder.AppendLine($"global::{Names.AddLangRegistrant}(\"{args[0].Value}\", "
                                                   + $"global::{Names.LangRegistrant}.Get<{genericTypes[0]}, {genericTypes[1]}>"
                                                   + $"({register.Name}));");
                            break;
                    }
                }
                sourceBuilder.AppendLine("")
                             .AppendLine("//manually register")
                             .AppendLine("_Register();");
            }
            sourceBuilder.AppendLine("}");

            sourceBuilder.AppendLine("")
                         .AppendLine("//manually register method")
                         .AppendLine("static partial void _Register();");
        }
        sourceBuilder.AppendLine("}");

        //add to source
        var classFullName = info.NameSpace is null or "" ? info.ClassName : $"{info.NameSpace}.{info.ClassName}";
        context.AddSource($"{classFullName}{Names.PartialFileSuffix}", sourceBuilder.ToString());
    }

    #endregion


    #region Global Generation

    private static void GlobalExecute(SourceProductionContext context, Compilation compilation,
                                      ImmutableArray<INamedTypeSymbol> classSymbols)
    {
        if (classSymbols.IsDefaultOrEmpty) return;

        //initialize
        IndentStringBuilder sourceBuilder = new();

        //generate
        sourceBuilder.AppendLine($"namespace {Names.GenerateNamespace}.{compilation.AssemblyName};")
                     .AppendLine("");
        sourceBuilder.AppendLine($"[global::System.CodeDom.Compiler.GeneratedCode(\"{Project.Name}\", \"{Project.Version}\")]")
                     .AppendLine($"public static class {Names.GeneratedClass}")
                     .AppendLine("{");

        using (sourceBuilder.Indent())
        {
            sourceBuilder.AppendLine("internal static void Register()")
                         .AppendLine("{");
            using (sourceBuilder.Indent())
            {
                bool noWhiteLine = true;
                foreach (var classSymbol in classSymbols)
                {
                    if (noWhiteLine) noWhiteLine = false;
                    else sourceBuilder.AppendLine("");
                    sourceBuilder.AppendLine($"global::{classSymbol.ToDisplayString()}.Register();");
                }
            }
            sourceBuilder.AppendLine("}");
        }

        sourceBuilder.AppendLine("}");

        //add to source
        context.AddSource(Names.TargetFile, sourceBuilder.ToString());
    }

    #endregion
}