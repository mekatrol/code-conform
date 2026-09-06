using CodeConform.CSharp.Analyzers.Rules;
using CodeConform.CSharp.Analyzers.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeConform.CSharp.Analyzers;

/// <summary>
/// Enforces separation before ordinary comment blocks that follow source items.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BlankLineBeforeCommentAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.BlankLineBeforeComment];

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

        foreach (var comment in root.DescendantTrivia(descendIntoTrivia: true))
        {
            if (!FormattingSyntax.IsOrdinaryComment(comment) ||
                !StartsOnOtherwiseEmptyLine(comment, sourceText) ||
                HasEarlierCommentInGroup(comment, sourceText))
            {
                continue;
            }

            var previousToken = comment.Token == default
                ? default
                : comment.Token.SpanStart < comment.SpanStart
                    ? comment.Token
                    : comment.Token.GetPreviousToken();

            if (previousToken == default ||
                previousToken.IsKind(SyntaxKind.OpenBraceToken) ||
                previousToken.IsKind(SyntaxKind.ColonToken) ||
                FormattingSyntax.ContainsBlankLine(
                    sourceText,
                    previousToken.Span.End,
                    comment.SpanStart) ||
                IsOwnedByFollowingStatement(comment, sourceText))
            {
                continue;
            }

            context.ReportDiagnostic(
                Diagnostic.Create(
                    DiagnosticDescriptors.BlankLineBeforeComment,
                    comment.GetLocation()));
        }
    }

    private static bool StartsOnOtherwiseEmptyLine(
        SyntaxTrivia comment,
        Microsoft.CodeAnalysis.Text.SourceText sourceText)
    {
        var line = sourceText.Lines.GetLineFromPosition(comment.SpanStart);
        var prefix = sourceText.ToString(
            Microsoft.CodeAnalysis.Text.TextSpan.FromBounds(
                line.Start,
                comment.SpanStart));

        return string.IsNullOrWhiteSpace(prefix);
    }

    private static bool HasEarlierCommentInGroup(
        SyntaxTrivia comment,
        Microsoft.CodeAnalysis.Text.SourceText sourceText)
    {
        var previousTrivia = comment.Token.LeadingTrivia.IndexOf(comment) is var leadingIndex &&
            leadingIndex > 0
                ? comment.Token.LeadingTrivia[leadingIndex - 1]
                : default;

        if (previousTrivia == default)
        {
            return false;
        }

        for (var position = leadingIndex - 1; position >= 0; position--)
        {
            var trivia = comment.Token.LeadingTrivia[position];

            if (FormattingSyntax.IsOrdinaryComment(trivia))
            {
                return !FormattingSyntax.ContainsBlankLine(
                    sourceText,
                    trivia.Span.End,
                    comment.SpanStart);
            }

            if (!trivia.IsKind(SyntaxKind.WhitespaceTrivia) &&
                !trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                break;
            }
        }

        return false;
    }

    private static bool IsOwnedByFollowingStatement(
        SyntaxTrivia comment,
        Microsoft.CodeAnalysis.Text.SourceText sourceText)
    {
        var token = comment.Token.SpanStart >= comment.Span.End
            ? comment.Token
            : comment.Token.GetNextToken();
        var statement = token.Parent?.FirstAncestorOrSelf<StatementSyntax>();

        if (statement is null ||
            FormattingSyntax.ContainsBlankLine(
                sourceText,
                comment.Span.End,
                statement.SpanStart))
        {
            return false;
        }

        return statement is ReturnStatementSyntax ||
            FormattingSyntax.IsBlockOpeningStatement(statement);
    }
}