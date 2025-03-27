using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis.CSharp.Syntax;

[Generator]
public class DBDefineGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        // 不需要初始化逻辑
    }

    public void Execute(GeneratorExecutionContext context)
    {
        var dbDefineClasses = new Dictionary<string, int>();
        var testDefineClasses = new Dictionary<string, int>();
        var cacheOnlyTypes = new HashSet<string>(); // 用于存储仅缓存的类型名称

        // 获取编译中的语法树
        var compilation = context.Compilation;

        // 获取 DBDefine 类型符号
        var dbDefineType = compilation.GetTypeByMetadataName("EdgeStudio.DB.DBDefine");
        if (dbDefineType == null)
        {
            // 如果 DBDefine 类型不存在，直接返回
            return;
        }

        // 获取 DBIndex Attribute 类型符号
        var dbIndexAttributeType = compilation.GetTypeByMetadataName("EdgeStudio.DB.DBIndexAttribute");
        // 获取 DBCacheOnly Attribute 类型符号
        var dbCacheOnlyAttributeType = compilation.GetTypeByMetadataName("EdgeStudio.DB.DBCacheOnlyAttribute");
        if (dbIndexAttributeType == null && dbCacheOnlyAttributeType == null)
        {
            // 如果 DBIndexAttribute 类型不存在，直接返回
            return;
        }

        // 遍历所有语法树
        foreach (var syntaxTree in compilation.SyntaxTrees)
        {
            var semanticModel = compilation.GetSemanticModel(syntaxTree);

            // 获取所有类声明
            var classDeclarations = syntaxTree.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>();

            foreach (var classDeclaration in classDeclarations)
            {
                var classSymbol = semanticModel.GetDeclaredSymbol(classDeclaration) as INamedTypeSymbol;

                // 检查类是否继承自 DBDefine
                if (classSymbol != null && classSymbol.BaseType != null && classSymbol.BaseType.Equals(dbDefineType, SymbolEqualityComparer.Default))
                {
                    // 检查类是否带有 DBCacheOnly Attribute
                    bool isCacheOnly = false;
                    if (dbCacheOnlyAttributeType != null)
                    {
                        isCacheOnly = classSymbol.GetAttributes()
                            .Any(attr => attr.AttributeClass?.Equals(dbCacheOnlyAttributeType, SymbolEqualityComparer.Default) == true);
                    }

                    // 如果是缓存类型，记录下来（但不排除它）
                    if (isCacheOnly)
                    {
                        cacheOnlyTypes.Add(classSymbol.Name);
                    }

                    // 检查类是否带有 DBIndex Attribute
                    var dbIndexAttribute = classSymbol.GetAttributes()
                        .FirstOrDefault(attr => attr.AttributeClass?.Equals(dbIndexAttributeType, SymbolEqualityComparer.Default) == true);
                    var className = classSymbol.Name;
                    if (dbIndexAttribute != null)
                    {
                        // 获取 DBIndex Attribute 的 Index 值
                        var indexArgument = dbIndexAttribute.ConstructorArguments.FirstOrDefault();
                        if (indexArgument.Kind == TypedConstantKind.Primitive && indexArgument.Value is int index)
                        {

                            // 根据 Index 值判断是添加到 dbDefineClasses 还是 testDefineClasses
                            if (index > 10000)
                            {
                                testDefineClasses.Add(className, index);
                            }
                            else
                            {
                                dbDefineClasses.Add(className, index);
                            }
                        }
                    }
                    else
                    {
                        dbDefineClasses.Add(className, 0);
                    }
                }
            }
        }

        if (dbDefineClasses.Count == 0 && testDefineClasses.Count == 0)
            return;

        var sourceBuilder = new StringBuilder();
        sourceBuilder.AppendLine("/// <summary>");
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// 以下代码是通过RemoteDBCollectionGenerator编辑器脚本自动生成.请勿手动修改本文件的代码" + cacheOnlyTypes.FirstOrDefault()?.ToString());
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// ⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠⚠");
        sourceBuilder.AppendLine("/// </summary>");
        sourceBuilder.AppendLine("using System;");
        sourceBuilder.AppendLine("using System.Collections.Generic;");
        sourceBuilder.AppendLine("#if USE_MEMORYPACK");
        sourceBuilder.AppendLine("using MemoryPack;");
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("#if USEJSON");
        sourceBuilder.AppendLine($"using Newtonsoft.Json;");
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("namespace EdgeStudio.DB");
        sourceBuilder.AppendLine("{");
        sourceBuilder.AppendLine("#if USE_MEMORYPACK");
        sourceBuilder.AppendLine("    [MemoryPackable(SerializeLayout.Explicit)]");
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("    public partial class RemoteDataCollection");
        sourceBuilder.AppendLine("    {");

        // 生成所有字段，为仅缓存类型添加忽略序列化标记
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine("#if USE_MEMORYPACK");
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine("        [MemoryPackIgnore]");
            }
            else
            {
                sourceBuilder.AppendLine($"        [MemoryPackOrder({type.Value})]");
            }
            sourceBuilder.AppendLine("#endif");
            sourceBuilder.AppendLine("#if USEJSON");
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine("        [JsonIgnore]");
            }
            else
            {
                sourceBuilder.AppendLine($"        [JsonProperty(\"{type.Key.Replace("Data_", "")}\")]");
            }
            sourceBuilder.AppendLine("#endif");
            
            // 添加注释标记仅缓存类型
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine($"        // 此字段为仅缓存类型，不会序列化");
            }
            sourceBuilder.AppendLine($"public {className} {className};");
        }
        
        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine("#if USE_MEMORYPACK");
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine("        [MemoryPackIgnore]");
            }
            else
            {
                sourceBuilder.AppendLine($"        [MemoryPackOrder({type.Value})]");
            }
            sourceBuilder.AppendLine("#endif");
            sourceBuilder.AppendLine("#if USEJSON");
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine("        [JsonIgnore]");
            }
            else
            {
                sourceBuilder.AppendLine($"        [JsonProperty(\"{type.Key.Replace("Data_", "")}\")]");
            }
            sourceBuilder.AppendLine("#endif");
            
            // 添加注释标记仅缓存类型
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine($"        // 此字段为仅缓存类型，不会序列化");
            }
            sourceBuilder.AppendLine($"public {className} {className};");
        }
        sourceBuilder.AppendLine("#endif");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public static IReadOnlyList<Type> AllType = new[]");
        sourceBuilder.AppendLine("        {");
        // 添加所有类型到AllType列表，包括仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine($"            typeof({className}), // 仅缓存类型");
            }
            else
            {
                sourceBuilder.AppendLine($"            typeof({className}),");
            }
        }

        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            if (cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine($"            typeof({className}), // 仅缓存类型");
            }
            else
            {
                sourceBuilder.AppendLine($"            typeof({className}),");
            }
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("        };");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public List<DBDefine> GetList() => new List<DBDefine>");
        sourceBuilder.AppendLine("        {");
        // GetList方法包含所有类型，包括仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            {className},");
        }

        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            {className},");
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("        };");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public T Get<T>() where T : DBDefine");
        sourceBuilder.AppendLine("        {");
        sourceBuilder.AppendLine("            var t = typeof(T);");
        sourceBuilder.AppendLine("            return Get(t) as T;");
        sourceBuilder.AppendLine("        }");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public DBDefine Get(Type t)");
        sourceBuilder.AppendLine("        {");
        // Get方法包含所有类型，包括仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if (t == typeof({className}))");
            sourceBuilder.AppendLine($"                return {className};");
        }
        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if (t == typeof({className}))");
            sourceBuilder.AppendLine($"                return {className};");
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("            return null;");
        sourceBuilder.AppendLine("        }");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public void Set<T>(T value) where T : DBDefine");
        sourceBuilder.AppendLine("        {");
        sourceBuilder.AppendLine("            var t = typeof(T);");
        // Set方法包含所有类型，包括仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if (t == typeof({className}))");
            sourceBuilder.AppendLine($"                {className} = ({className})(object)value;");
        }
        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if (t == typeof({className}))");
            sourceBuilder.AppendLine($"                {className} = ({className})(object)value;");
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("        }");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public void Set(DBDefine value)");
        sourceBuilder.AppendLine("        {");
        sourceBuilder.AppendLine("            var t = value.GetType();");
        // Set方法包含所有类型，包括仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if (t == typeof({className}))");
            sourceBuilder.AppendLine($"                {className} = ({className})(object)value;");
        }
        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if (t == typeof({className}))");
            sourceBuilder.AppendLine($"                {className} = ({className})(object)value;");
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("        }");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public void Init()");
        sourceBuilder.AppendLine("        {");
        // Init方法包含所有类型，包括仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            {className}?.Init();");
        }
        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            {className}?.Init();");
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("        }");

        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("#if !NET");
        sourceBuilder.AppendLine("        public void LoadFromData()");
        sourceBuilder.AppendLine("        {");
        // LoadFromData方法跳过仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            if (!cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine($"            {className} = DBManager.Inst.Query<{className}>();");
            }
        }
        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            if (!cacheOnlyTypes.Contains(className))
            {
                sourceBuilder.AppendLine($"            {className} = DBManager.Inst.Query<{className}>();");
            }
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("        }");
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine();
        sourceBuilder.AppendLine("        public void IfNullNewData()");
        sourceBuilder.AppendLine("        {");
        // IfNullNewData方法包含所有类型，包括仅缓存类型
        foreach (var type in dbDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if ({className} == null)");
            sourceBuilder.AppendLine("            {");
            sourceBuilder.AppendLine($"                {className} = new {className}();");
            sourceBuilder.AppendLine($"                {className}.NewData();");
            sourceBuilder.AppendLine($"                {className}.Init();");
            sourceBuilder.AppendLine("            }");
        }
        sourceBuilder.AppendLine("#if UNITY_EDITOR || TEST");
        foreach (var type in testDefineClasses)
        {
            var className = type.Key;
            sourceBuilder.AppendLine($"            if ({className} == null)");
            sourceBuilder.AppendLine("            {");
            sourceBuilder.AppendLine($"                {className} = new {className}();");
            sourceBuilder.AppendLine($"                {className}.NewData();");
            sourceBuilder.AppendLine($"                {className}.Init();");
            sourceBuilder.AppendLine("            }");
        }
        sourceBuilder.AppendLine("#endif");
        sourceBuilder.AppendLine("        }");

        sourceBuilder.AppendLine("    }");
        sourceBuilder.AppendLine("}");

        // 将生成的代码添加到编译上下文中
        context.AddSource("RemoteDataCollection.generated.cs", SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
    }
}