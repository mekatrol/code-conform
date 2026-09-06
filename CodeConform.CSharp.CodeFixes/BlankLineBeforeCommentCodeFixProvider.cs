using CodeConform.CSharp.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using System.Collections.Immutable;
using System.Composition;
using System.Threading.Tasks;

namespace CodeConform.CSharp.CodeFixes;

/// <summary>
/// Inserts the separator required by CC0004 before an ordinary comment block.
/// </summary>
[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(BlankLineBeforeCommentCodeFixProvider))]
[Shared]
public sealed class BlankLineBeforeCommentCodeFixProvider : CodeFixProvider
{
    /// <inheritdoc />
    public override ImmutableArray<string> FixableDiagnosticIds =>
        [DiagnosticIds.BlankLineBeforeComment];

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
        var comment = root.FindTrivia(
            diagnostic.Location.SourceSpan.Start,
            findInsideTrivia: true);

        if (comment == default)
        {
            return;
        }

        context.RegisterCodeFix(
            CodeAction.Create(
                "Insert blank line before comment block",
                cancellationToken => FormattingCodeFix.InsertBlankLineAsync(
                    context.Document,
                    comment.SpanStart,
                    cancellationToken),
                DiagnosticIds.BlankLineBeforeComment),
            diagnostic);
    }
}