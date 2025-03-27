using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StringLowerCase
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(MustLowerCodeFixProvider)), Shared]
    public class MustLowerCodeFixProvider : CodeFixProvider
    {
        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(StringLowerCaseAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            var token = root?.FindToken(diagnosticSpan.Start);

            if (token?.Parent is LiteralExpressionSyntax stringLiteral)
            {
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: "转换成小写",
                        createChangedDocument: c => ConvertToLowercaseAsync(context.Document, stringLiteral, c),
                        equivalenceKey: "Convert to lowercase"),
                    diagnostic);
            }
        }

        private async Task<Document> ConvertToLowercaseAsync(Document document, LiteralExpressionSyntax stringLiteral, CancellationToken cancellationToken)
        {
            var newValue = "\"" + stringLiteral.Token.ValueText.ToLowerInvariant() + "\"";
            var newLiteral = SyntaxFactory.Literal(stringLiteral.Token.LeadingTrivia, newValue, stringLiteral.Token.ValueText.ToLowerInvariant(),
                stringLiteral.Token.TrailingTrivia);

            var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
            var newRoot = root?.ReplaceToken(stringLiteral.Token, newLiteral);

            return newRoot != null ? document.WithSyntaxRoot(newRoot) : document;
        }
    }
}
