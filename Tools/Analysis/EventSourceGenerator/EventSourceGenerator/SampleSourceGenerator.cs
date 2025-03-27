using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace EventGenerator
{
    [Generator]
    public class EventStructGenerator : IIncrementalGenerator
    {
        // 游戏事件属性的完全限定名
        private const string GameEventAttributeName = "EdgeStudio.Event.GameEventAttribute";

        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            // 注册语法提供程序，筛选出所有结构体
            var structDeclarations = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: static (s, _) => s is StructDeclarationSyntax,
                    transform: static (ctx, _) => (StructDeclarationSyntax)ctx.Node)
                .Where(static m => m is not null);

            // 获取编译上下文以访问语义模型
            var compilationProvider = context.CompilationProvider;
            
            // 将结构声明与编译上下文结合
            var structsToProcess = structDeclarations.Combine(compilationProvider);

            // 注册源代码输出
            context.RegisterSourceOutput(structsToProcess, 
                static (spc, tuple) => Execute(spc, tuple.Left, tuple.Right));

            // 生成GameEventAttribute
            context.RegisterSourceOutput(compilationProvider, GenerateGameEventAttribute);
        }

        private static void GenerateGameEventAttribute(SourceProductionContext context, Compilation compilation)
        {
            // 生成GameEventAttribute代码
            string attributeSource = @"
using System;

namespace EdgeStudio.Event
{
    /// <summary>
    /// 标记一个结构体为游戏事件。
    /// 标记了此特性的结构体将自动生成事件基础设施代码。
    /// </summary>
    [AttributeUsage(AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class GameEventAttribute : Attribute
    {
        public GameEventAttribute()
        {
        }
    }
}";
            
            context.AddSource("GameEventAttribute.g.cs", SourceText.From(attributeSource, Encoding.UTF8));
        }

        private static void Execute(SourceProductionContext context, StructDeclarationSyntax structDeclaration, Compilation compilation)
        {
            // 获取语义模型
            var semanticModel = compilation.GetSemanticModel(structDeclaration.SyntaxTree);
            
            // 获取结构体符号
            var structSymbol = semanticModel.GetDeclaredSymbol(structDeclaration);
            if (structSymbol == null)
                return;

            // 确保结构体是公共的
            if (structSymbol.DeclaredAccessibility != Accessibility.Public)
                return;

            // 检查是否标记了GameEventAttribute
            bool hasGameEventAttribute = HasAttribute(structSymbol, GameEventAttributeName, compilation);
            if (!hasGameEventAttribute)
                return;

            // 获取结构体名称
            string structName = structSymbol.Name;
            
            // 获取命名空间
            string namespaceName = structSymbol.ContainingNamespace.ToDisplayString();
            bool isFileScoped = IsFileScopedNamespace(structDeclaration);

            // 获取公共成员（字段和属性）
            var publicMembers = GetPublicMembers(structSymbol);

            // 生成Trigger方法参数和赋值语句
            string parameters = GenerateTriggerParameters(publicMembers);
            string assignments = GenerateTriggerAssignments(publicMembers);

            // 生成分部结构体代码
            string source = GeneratePartialStruct(namespaceName, structName, parameters, assignments, isFileScoped);
            
            // 添加源代码
            context.AddSource($"{structName}.g.cs", SourceText.From(source, Encoding.UTF8));
        }

        private static bool HasAttribute(ISymbol symbol, string attributeName, Compilation compilation)
        {
            // 推导简写形式的特性名称（去掉Attribute后缀）
            string shortFormName = attributeName;
            if (attributeName.EndsWith("Attribute"))
            {
                shortFormName = attributeName.Substring(0, attributeName.Length - "Attribute".Length);
            }

            // 检查是否具有指定的属性
            foreach (var attribute in symbol.GetAttributes())
            {
                var attributeClass = attribute.AttributeClass;
                if (attributeClass == null)
                    continue;

                // 获取特性的完全限定名称
                string fullName = attributeClass.ToDisplayString();
                
                // 检查完全限定名称（包括带Attribute后缀和不带后缀的版本）
                if (fullName == attributeName || fullName == shortFormName)
                    return true;
                    
                // 获取特性的简单名称（不包含命名空间）
                string simpleName = attributeClass.Name;
                
                // 检查是否匹配简单名称的任一形式
                if (simpleName == "GameEventAttribute" || simpleName == "GameEvent")
                    return true;
                    
                // 检查特性名称是否以GameEvent开头并以Attribute结尾
                // 这有助于捕获不同命名空间中的类似特性
                if (simpleName.StartsWith("GameEvent") && simpleName.EndsWith("Attribute"))
                    return true;
            }

            return false;
        }

        private static bool IsFileScopedNamespace(StructDeclarationSyntax structDecl)
        {
            // 检查结构体是否在文件作用域命名空间内
            SyntaxNode parent = structDecl.Parent;
            while (parent != null)
            {
                if (parent is FileScopedNamespaceDeclarationSyntax)
                    return true;
                parent = parent.Parent;
            }
            return false;
        }

        private static List<(string Type, string Name)> GetPublicMembers(INamedTypeSymbol structSymbol)
        {
            var members = new List<(string Type, string Name)>();

            // 添加公共字段
            foreach (var member in structSymbol.GetMembers().OfType<IFieldSymbol>())
            {
                if (member.DeclaredAccessibility == Accessibility.Public && !member.IsStatic)
                {
                    members.Add((member.Type.ToDisplayString(), member.Name));
                }
            }

            // 添加公共属性
            foreach (var member in structSymbol.GetMembers().OfType<IPropertySymbol>())
            {
                if (member.DeclaredAccessibility == Accessibility.Public && !member.IsStatic)
                {
                    members.Add((member.Type.ToDisplayString(), member.Name));
                }
            }

            return members;
        }

        private static string GenerateTriggerParameters(List<(string Type, string Name)> members)
        {
            var parameters = new StringBuilder();
            bool isFirst = true;

            foreach (var (type, name) in members)
            {
                if (!isFirst)
                    parameters.Append(", ");
                else
                    isFirst = false;

                var paramName = char.ToLowerInvariant(name[0]) + name.Substring(1);
                parameters.Append($"{type} {paramName} = default");
            }

            return parameters.ToString();
        }

        private static string GenerateTriggerAssignments(List<(string Type, string Name)> members)
        {
            var assignments = new StringBuilder();

            foreach (var (_, name) in members)
            {
                var paramName = char.ToLowerInvariant(name[0]) + name.Substring(1);
                assignments.AppendLine($"            e.{name} = {paramName};");
            }

            return assignments.ToString();
        }

        private static string GeneratePartialStruct(string namespaceName, string structName, string parameters, string assignments, bool isFileScoped)
        {
            string namespaceDeclaration;
            string namespaceClosing;

            if (string.IsNullOrEmpty(namespaceName))
            {
                namespaceDeclaration = string.Empty;
                namespaceClosing = string.Empty;
            }
            else if (isFileScoped)
            {
                namespaceDeclaration = $"namespace {namespaceName};";
                namespaceClosing = string.Empty;
            }
            else
            {
                namespaceDeclaration = $"namespace {namespaceName}\n{{";
                namespaceClosing = "}";
            }

            return $@"// <auto-generated/>
using R3;
using UnityEngine;

{namespaceDeclaration}
    public partial struct {structName}
    {{
        private static {structName} e; // 静态事件实例
        private static Subject<{structName}> mEvent = new(); // 事件主题
        public static Observable<{structName}> Event => mEvent; // 事件观察者
    
#if UNITY_EDITOR
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RuntimeInitialization()
        {{
            mEvent.OnCompleted();
            mEvent = new Subject<{structName}>();
        }}
#endif
    
        // 触发事件
        public static void Trigger({parameters})
        {{
{assignments}            mEvent.OnNext(e);
        }}
    }}
{namespaceClosing}
";
        }
    }
}