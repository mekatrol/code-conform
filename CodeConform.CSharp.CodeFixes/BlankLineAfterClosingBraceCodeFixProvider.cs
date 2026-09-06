using CodeConform.CSharp.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace CodeConform.CSharp.CodeFixes;

/// <summary>
/// Inserts the separator required by CC0003 after a semantic closing brace.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BlankLineAfterClosingBraceCodeFixProvider))]
[Shared]
public sealed class BlankLineAfterClosingBraceCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticIds.BlankLineAfterClosingBrace];

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
        var closingBrace = root.FindToken(diagnostic.Location.SourceSpan.Start);
        var nextToken = closingBrace.GetNextToken();

        if (nextToken.RawKind == 0)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Insert blank line after closing brace",
                cancellationToken => FormattingCodeFix.InsertBlankLineAsync(
                    context.Document,
                    nextToken.SpanStart,
                    cancellationToken),
                DiagnosticIds.BlankLineAfterClosingBrace),
            diagnostic);
    }
}