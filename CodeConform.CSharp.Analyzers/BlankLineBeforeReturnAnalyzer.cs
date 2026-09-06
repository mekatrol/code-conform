using CodeConform.CSharp.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using System.Collections.Immutable;

namespace CodeConform.CSharp.Analyzers;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class BlankLineBeforeReturnAnalyzer : DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        [DiagnosticDescriptors.BlankLineBeforeReturn];

    public override void Initialize(AnalysisContext context)
    {
        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();

        context.RegisterSyntaxNodeAction(
            AnalyzeReturnStatement,
            SyntaxKind.ReturnStatement);
    }

    private static void AnalyzeReturnStatement(SyntaxNodeAnalysisContext context)
    {
        var returnStatement = (ReturnStatementSyntax)context.Node;

        if (returnStatement.Parent is not BlockSyntax block)
        {
            return;
        }

        var blockLineSpan = block.SyntaxTree.GetLineSpan(block.Span);

        if (blockLineSpan.StartLinePosition.Line == blockLineSpan.EndLinePosition.Line)
        {
            return;
        }

        var index = block.Statements.IndexOf(returnStatement);

        if (index <= 0)
        {
            return;
        }

        var previousStatement = block.Statements[index - 1];
        var sourceText = context.Node.SyntaxTree.GetText(context.CancellationToken);

        var previousLine = sourceText.Lines.GetLineFromPosition(
            previousStatement.Span.End).LineNumber;

        var returnLine = sourceText.Lines.GetLineFromPosition(
            returnStatement.SpanStart).LineNumber;

        for (var lineNumber = previousLine + 1;
            lineNumber < returnLine;
            lineNumber++)
        {
            var line = sourceText.Lines[lineNumber];

            if (string.IsNullOrWhiteSpace(line.ToString()))
            {
                return;
            }
        }

        context.ReportDiagnostic(
            Diagnostic.Create(
                DiagnosticDescriptors.BlankLineBeforeReturn,
                returnStatement.GetLocation()));
    }
}
