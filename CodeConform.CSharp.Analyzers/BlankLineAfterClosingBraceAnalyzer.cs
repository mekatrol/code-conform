using CodeConform.CSharp.Analyzers.Rules;
using CodeConform.CSharp.Analyzers.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;
using System.Linq;

namespace CodeConform.CSharp.Analyzers;

/// <summary>
/// Enforces separation after closing braces that complete semantic bodies.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BlankLineAfterClosingBraceAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.BlankLineAfterClosingBrace];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxTreeAction(AnalyzeTree);
    }

    private static void AnalyzeTree(SyntaxTreeAnalysisContext context)
    {
        var root = context.Tree.GetRoot(context.CancellationToken);
        var sourceText = context.Tree.GetText(context.CancellationToken);

        foreach (var closingBrace in root.DescendantTokens().Where(
            token => FormattingSyntax.IsSemanticClosingBrace(token)))
        {
            var nextToken = closingBrace.GetNextToken();

            if (FormattingSyntax.IsClosingBraceContinuation(closingBrace, nextToken) ||
                FormattingSyntax.ContainsBlankLine(
                    sourceText,
                    closingBrace.Span.End,
                    nextToken.SpanStart) ||
                HasOrdinaryCommentBetween(closingBrace, nextToken) ||
                IsOwnedByFollowingStatement(nextToken))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.BlankLineAfterClosingBrace,
                    closingBrace.GetLocation()));
        }
    }

    private static bool HasOrdinaryCommentBetween(
        SyntaxToken closingBrace,
        SyntaxToken nextToken)
    {
        return closingBrace.TrailingTrivia
                .Concat(nextToken.LeadingTrivia)
                .Any(FormattingSyntax.IsOrdinaryComment);
    }

    private static bool IsOwnedByFollowingStatement(SyntaxToken nextToken)
    {
        var statement = nextToken.Parent?.FirstAncestorOrSelf<StatementSyntax>();

        return statement is ReturnStatementSyntax ||
            statement is not null && FormattingSyntax.IsBlockOpeningStatement(statement);
    }
}