using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Rename;
using Microsoft.CodeAnalysis.Text;

namespace QuoterAnalyzer
{
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(QuoterAnalyzerCodeFixProvider)), Shared]
    public class QuoterAnalyzerCodeFixProvider : CodeFixProvider
    {
        private const string title = "Parse argument to SyntaxFactory.Parse";

        public sealed override ImmutableArray<string> FixableDiagnosticIds
        {
            get { return ImmutableArray.Create(QuoterAnalyzerAnalyzer.DiagnosticId); }
        }

        public sealed override FixAllProvider GetFixAllProvider()
        {
            // See https://github.com/dotnet/roslyn/blob/master/docs/analyzers/FixAllProvider.md for more information on Fix All Providers
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            // TODO: Replace the following code with your own analysis, generating a CodeAction for each fix to suggest
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;

            // Find the type declaration identified by the diagnostic.
            var invocation = (InvocationExpressionSyntax)root.FindNode(diagnosticSpan, getInnermostNodeForTie: false);
            var argumentExpression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            
            if (argumentExpression != null
                && argumentExpression.Kind() == SyntaxKind.StringLiteralExpression)
            {
                // Register a code action that will invoke the fix.
                context.RegisterCodeFix(
                    CodeAction.Create(
                        title: title,
                        createChangedDocument: c => MakeUppercaseAsync(context.Document, invocation, c),
                        equivalenceKey: title),
                    diagnostic);
            }

        }

        private async Task<Document> MakeUppercaseAsync(Document document, InvocationExpressionSyntax invocation, CancellationToken cancellationToken)
        {
            var argumentExpression = invocation.ArgumentList.Arguments.FirstOrDefault()?.Expression;
            var argumentContents = argumentExpression.ToString().Replace("\"", string.Empty);

            var quoter = new Quoter();
            var roslynCalls = SyntaxFactory.ParseExpression(quoter.Quote(argumentContents));
            var newRoot = (await document.GetSyntaxRootAsync()).ReplaceNode(invocation, roslynCalls);
            var newDocument = document.WithSyntaxRoot(newRoot);
            
            return newDocument;
        }
    }
}