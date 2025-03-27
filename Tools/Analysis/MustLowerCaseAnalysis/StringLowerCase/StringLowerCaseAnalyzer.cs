using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace StringLowerCase
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StringLowerCaseAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "StringLowerCase";

        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString MessageFormat =
            new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));

        private static readonly LocalizableString Description =
            new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));

        private const string Category = "Naming";

        private static readonly DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true,
            description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context)
        {
            /*if (!Debugger.IsAttached) {
                Debugger.Launch();
            }*/
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();

            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.InvocationExpression);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var invocationExpr = (InvocationExpressionSyntax)context.Node;
            var semanticModel = context.SemanticModel;

            if (semanticModel.GetSymbolInfo(invocationExpr).Symbol is IMethodSymbol methodSymbol)
            {
                var parameters = methodSymbol.Parameters;
                for (int i = 0; i < parameters.Length; i++)
                {
                    var parameter = parameters[i];
                    if (parameter.GetAttributes().Any(attr => attr?.AttributeClass?.Name.EndsWith("MustLowerAttribute") ?? false) && invocationExpr.ArgumentList.Arguments.Count > i)
                    {
                        var argument = invocationExpr.ArgumentList.Arguments[i];
                        var argumentValue = semanticModel.GetConstantValue(argument.Expression);

                        if (argumentValue.HasValue && argumentValue.Value is string strValue && strValue.Any(char.IsUpper))
                        {
                            var diagnostic = Diagnostic.Create(Rule, argument.GetLocation(), argument.Expression.ToString(), parameter.Name);
                            context.ReportDiagnostic(diagnostic);
                        }
                    }
                }
            }
        }
        
        /*private static void AnalyzeSymbol(SymbolAnalysisContext context)
        {
            var namedTypeSymbol = (INamedTypeSymbol)context.Symbol;

            if (namedTypeSymbol.Name.ToCharArray().Any(char.IsUpper))
            {
                var diagnostic = Diagnostic.Create(Rule, namedTypeSymbol.Locations[0], namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }*/
    }
}
