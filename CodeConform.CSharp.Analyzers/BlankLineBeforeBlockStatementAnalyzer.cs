using CodeConform.CSharp.Analyzers.Rules;
using CodeConform.CSharp.Analyzers.Syntax;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeConform.CSharp.Analyzers;

/// <summary>
/// Enforces separation before executable statements that open a block.
/// </summary>
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BlankLineBeforeBlockStatementAnalyzer : DiagnosticAnalyzer
{
    /// <inheritdoc />
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.BlankLineBeforeBlockStatement];

    /// <inheritdoc />
    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSyntaxNodeAction(
            AnalyzeStatement,
            SyntaxKind.IfStatement,
            SyntaxKind.ForStatement,
            SyntaxKind.ForEachStatement,
            SyntaxKind.ForEachVariableStatement,
            SyntaxKind.WhileStatement,
            SyntaxKind.DoStatement,
            SyntaxKind.SwitchStatement,
            SyntaxKind.TryStatement,
            SyntaxKind.UsingStatement,
            SyntaxKind.LockStatement,
            SyntaxKind.FixedStatement);
    }

    private static void AnalyzeStatement(SyntaxNodeAnalysisContext context)
    {
        var statement = (StatementSyntax)context.Node;

        if (!FormattingSyntax.IsBlockOpeningStatement(statement))
        {
            return;
        }

        var previousStatement = FormattingSyntax.GetPreviousStatement(statement);

        if (previousStatement is null)
        {
            return;
        }

        var sourceText = statement.SyntaxTree.GetText(context.CancellationToken);
        var groupStart = FormattingSyntax.GetDocumentedGroupStart(statement);

        if (FormattingSyntax.ContainsBlankLine(
            sourceText,
            previousStatement.Span.End,
            groupStart))
        {
            return;
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.BlankLineBeforeBlockStatement,
                statement.GetLocation()));
    }
}