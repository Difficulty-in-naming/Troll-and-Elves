using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator(LanguageNames.CSharp)]
public class ConfigLoaderGenerator : IIncrementalGenerator
{
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // 注册语法提供器，只关注类声明
        IncrementalValuesProvider<ClassDeclarationSyntax> classDeclarations = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
            .Where(static m => m is not null)!;

        // 将配置类信息合并到一个集合中
        IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndClasses
            = context.CompilationProvider.Combine(classDeclarations.Collect());

        // 注册源代码生成
        context.RegisterSourceOutput(compilationAndClasses, 
            static (spc, source) => Execute(source.Item1, source.Item2, spc));
    }

    private static bool IsSyntaxTargetForGeneration(SyntaxNode node)
        => node is ClassDeclarationSyntax { AttributeLists: { Count: > 0 } };

    private static ClassDeclarationSyntax? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclaration);

        if (symbol == null) return null;
        
        // 跳过ConfigAssetManager
        if (symbol.Name == "ConfigAssetManager") return null;

        // 检查是否实现IConfig接口
        if (!ImplementsIConfig(symbol)) return null;

        // 检查是否有符合要求的Load方法
        if (!HasValidLoadMethod(classDeclaration)) return null;

        return classDeclaration;
    }

    private static void Execute(Compilation compilation, ImmutableArray<ClassDeclarationSyntax> classes, SourceProductionContext context)
    {
        if (classes.IsDefaultOrEmpty) return;

        var configTypes = new List<string>();
        
        foreach (var classDeclaration in classes)
        {
            var semanticModel = compilation.GetSemanticModel(classDeclaration.SyntaxTree);
            var typeSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;
            
            if (typeSymbol != null)
            {
                configTypes.Add(typeSymbol.Name);
            }
        }

        // 生成加载器代码
        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine(@"using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace EdgeStudio.Config
{
    public static class ConfigLoader
    {
        public static async UniTask LoadAllConfigs()
        {
            var list = new List<UniTask>();");

        foreach (var type in configTypes)
        {
            sourceBuilder.AppendLine($"            list.Add({type}.Load());");
        }

        sourceBuilder.AppendLine(@"            await list;
        }
    }
}");

        context.AddSource("ConfigLoader.g.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }

    private static bool HasValidLoadMethod(ClassDeclarationSyntax classDeclaration)
    {
        foreach (var member in classDeclaration.Members.OfType<MethodDeclarationSyntax>())
        {
            if (member.Identifier.Text != "Load") continue;

            // 检查方法修饰符
            bool isPublic = member.Modifiers.Any(m => m.IsKind(SyntaxKind.PublicKeyword));
            bool isStatic = member.Modifiers.Any(m => m.IsKind(SyntaxKind.StaticKeyword));
            bool isAsync = member.Modifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword));

            // 检查返回类型
            bool returnsUniTask = member.ReturnType is IdentifierNameSyntax returnType &&
                                returnType.Identifier.Text == "UniTask";

            // 检查参数列表为空
            bool hasNoParameters = !member.ParameterList.Parameters.Any();

            if (isPublic && isStatic && isAsync && returnsUniTask && hasNoParameters)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ImplementsIConfig(ISymbol typeSymbol)
    {
        if (typeSymbol is not INamedTypeSymbol namedTypeSymbol) return false;

        // 检查直接接口
        foreach (var @interface in namedTypeSymbol.Interfaces)
        {
            if (@interface.Name == "IConfig")
                return true;
        }

        // 检查基类
        if (namedTypeSymbol.BaseType != null)
        {
            return ImplementsIConfig(namedTypeSymbol.BaseType);
        }

        return false;
    }
}