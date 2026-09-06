using CodeConform.CSharp.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace CodeConform.CSharp.CodeFixes;

/// <summary>
/// Inserts the separator required by CC0002 before a block-opening statement.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BlankLineBeforeBlockStatementCodeFixProvider))]
[Shared]
public sealed class BlankLineBeforeBlockStatementCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticIds.BlankLineBeforeBlockStatement];

    /// <inheritdoc />
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <inheritdoc />
    public override async Task RegisterCodeFixesAsync(CodeFixContext context)
    {
        var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken)
            .ConfigureAwait(false);

        if (root is null)
        {
            return;
        }

        var diagnostic = context.Diagnostics[0];
        var statement = root.FindNode(diagnostic.Location.SourceSpan)
            .FirstAncestorOrSelf<StatementSyntax>();

        if (statement is null)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Insert blank line before block-opening statement",
                cancellationToken => FormattingCodeFix.InsertBlankLineAsync(
                    context.Document,
                    FormattingCodeFix.GetDocumentedGroupStart(statement),
                    cancellationToken),
                DiagnosticIds.BlankLineBeforeBlockStatement),
            diagnostic);
    }
}