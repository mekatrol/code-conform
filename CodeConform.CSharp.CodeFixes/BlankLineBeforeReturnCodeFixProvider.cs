using CodeConform.CSharp.Analyzers.Rules;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

namespace CodeConform.CSharp.CodeFixes;

/// <summary>
/// Provides the automatic code fix for <c>CC0001</c>.
/// </summary>
/// <remarks>
/// <para>
/// <c>CC0001</c> requires a blank line before a <c>return</c> statement when
/// the return is not the first statement in its containing block.
/// </para>
/// <para>
/// This provider complements <see cref="Analyzers.BlankLineBeforeReturnAnalyzer"/>
/// by allowing Roslyn hosts such as Visual Studio and <c>dotnet format</c> to
/// automatically correct a reported violation.
/// </para>
/// <para>
/// The fix preserves the newline convention of the document being modified.
/// It must not depend on the operating system hosting the analyzer because the
/// package can be used in repositories with CRLF, LF, or CR line endings on
/// any supported operating system.
/// </para>
/// </remarks>
[ExportCodeFixProvider(
    LanguageNames.CSharp,
    Name = nameof(BlankLineBeforeReturnCodeFixProvider))]
[Shared]
public sealed class BlankLineBeforeReturnCodeFixProvider : CodeFixProvider
{
    /// <summary>
    /// Gets the diagnostic IDs that this provider is capable of fixing.
    /// </summary>
    /// <remarks>
    /// Restricting the provider to CC0001 ensures Roslyn only offers this fix
    /// for diagnostics whose formatting semantics are understood by this
    /// implementation.
    /// </remarks>
    public override ImmutableArray<string> FixableDiagnosticIds => [DiagnosticIds.BlankLineBeforeReturn];

    /// <summary>
    /// Gets the provider used by Roslyn to apply this fix to multiple
    /// diagnostics in a single operation.
    /// </summary>
    /// <returns>
    /// Roslyn's standard batch fixer.
    /// </returns>
    /// <remarks>
    /// Supporting Fix All allows the same deterministic transformation to be
    /// applied at document, project, or solution scope. This is also important
    /// when the analyzer is used by repository-wide formatting tooling.
    /// </remarks>
    public override FixAllProvider GetFixAllProvider()
    {
        return WellKnownFixAllProviders.BatchFixer;
    }

    /// <summary>
    /// Registers the automatic fix for a reported CC0001 diagnostic.
    /// </summary>
    /// <param name="context">
    /// The Roslyn code-fix context containing the document and diagnostics for
    /// which fixes are being requested.
    /// </param>
    /// <returns>
    /// A task representing the asynchronous registration operation.
    /// </returns>
    /// <remarks>
    /// Registration does not modify the document. It identifies the
    /// <see cref="ReturnStatementSyntax"/> associated with the diagnostic and
    /// registers a <see cref="CodeAction"/> that Roslyn may execute later if
    /// the user or formatting host elects to apply the fix.
    /// </remarks>
    public override async Task RegisterCodeFixesAsync(
        CodeFixContext context)
    {
        // Work against Roslyn's syntax tree rather than parsing source text.
        // This ensures that the diagnostic is associated with the actual
        // return statement reported by the analyzer.
        var root = await context.Document.GetSyntaxRootAsync(
            context.CancellationToken).ConfigureAwait(false);

        // A document can legitimately have no syntax root. In that situation
        // there is no syntax node against which a code fix can be registered.
        if (root is null)
        {
            return;
        }

        // Roslyn invokes this provider only for diagnostic IDs declared by
        // FixableDiagnosticIds. CC0001 currently reports one return statement
        // per diagnostic, so the first diagnostic identifies the target.
        var diagnostic = context.Diagnostics[0];

        // The diagnostic currently spans the complete return statement.
        // FindNode locates the syntax associated with that span, while
        // FirstAncestorOrSelf makes this robust if Roslyn returns a child node
        // within the diagnostic span rather than ReturnStatementSyntax itself.
        var returnStatement = root
            .FindNode(diagnostic.Location.SourceSpan)
            .FirstAncestorOrSelf<ReturnStatementSyntax>();

        // Do not offer a fix when the expected syntax cannot be located.
        // Applying a textual modification without a confirmed return statement
        // could change unrelated source.
        if (returnStatement is null)
        {
            return;
        }

        // Register the transformation rather than applying it immediately.
        // Roslyn controls when and how the action is executed, including
        // interactive light-bulb fixes and Fix All operations.
        context.RegisterCodeFix(
            CodeAction.Create(
                "Insert blank line before return statement",
                cancellationToken => InsertBlankLineAsync(
                    context.Document,
                    returnStatement,
                    cancellationToken),
                equivalenceKey: DiagnosticIds.BlankLineBeforeReturn),
            diagnostic);
    }

    /// <summary>
    /// Inserts the blank-line separator required by CC0001 before the specified
    /// return statement or before its immediately preceding documenting comments.
    /// </summary>
    /// <param name="document">
    /// The document containing the return statement.
    /// </param>
    /// <param name="returnStatement">
    /// The return statement associated with the CC0001 diagnostic.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// A new Roslyn document containing the corrected source text.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Roslyn documents and source text are immutable. The original document
    /// is therefore never modified. A new <see cref="SourceText"/> and
    /// <see cref="Document"/> are returned instead.
    /// </para>
    /// <para>
    /// The insertion location is determined by
    /// <see cref="GetInsertionPosition(ReturnStatementSyntax, SourceText)"/>.
    /// When the return statement is immediately preceded by one or more
    /// documenting comments, the separator is inserted before the entire
    /// contiguous comment block. Otherwise, it is inserted immediately before
    /// the return statement.
    /// </para>
    /// <para>
    /// The inserted text consists only of an additional line ending. Existing
    /// indentation, comments and statement contents remain unchanged.
    /// </para>
    /// </remarks>
    private static async Task<Document> InsertBlankLineAsync(
        Document document,
        ReturnStatementSyntax returnStatement,
        CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(
            cancellationToken).ConfigureAwait(false);

        var insertionPosition = GetInsertionPosition(
            returnStatement,
            sourceText);

        var insertionLine = sourceText.Lines.GetLineFromPosition(
            insertionPosition);

        var lineBreak = await GetLineBreakAsync(
            document,
            sourceText,
            insertionLine.LineNumber,
            cancellationToken).ConfigureAwait(false);

        var newText = sourceText.WithChanges(
            new TextChange(
                new TextSpan(insertionLine.Start, 0),
                lineBreak));

        return document.WithText(newText);
    }

    /// <summary>
    /// Determines the source position at which the CC0001 blank-line separator
    /// should be inserted for the specified return statement.
    /// </summary>
    /// <param name="returnStatement">
    /// The return statement for which an insertion position is required.
    /// </param>
    /// <param name="sourceText">
    /// The complete source text containing the return statement.
    /// </param>
    /// <returns>
    /// The source position at the start of the line before which the blank-line
    /// separator should be inserted.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The method examines the return statement's leading Roslyn trivia in reverse
    /// order so that comments immediately associated with the return can be
    /// treated as part of the same logical documentation block.
    /// </para>
    /// <para>
    /// Whitespace and end-of-line trivia are ignored while walking backwards.
    /// Consecutive single-line and multiline comments are recorded as documenting
    /// comments. The earliest comment in that contiguous sequence becomes the
    /// insertion target.
    /// </para>
    /// <para>
    /// If no documenting comment is found, the return statement itself is used as
    /// the insertion target. This causes the blank line to be inserted immediately
    /// before the return statement.
    /// </para>
    /// <para>
    /// Returning the start of the containing source line ensures that the inserted
    /// separator appears before the existing indentation rather than splitting
    /// indentation or comment text.
    /// </para>
    /// </remarks>
    private static int GetInsertionPosition(
        ReturnStatementSyntax returnStatement,
        SourceText sourceText)
    {
        var leadingTrivia = returnStatement.GetLeadingTrivia();

        if (leadingTrivia.Count == 0)
        {
            return returnStatement.SpanStart;
        }

        SyntaxTrivia? firstDocumentingComment = null;

        for (var index = leadingTrivia.Count - 1;
            index >= 0;
            index--)
        {
            var trivia = leadingTrivia[index];

            if (trivia.IsKind(SyntaxKind.WhitespaceTrivia) ||
                trivia.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                trivia.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                firstDocumentingComment = trivia;

                continue;
            }

            break;
        }

        if (firstDocumentingComment is null)
        {
            return returnStatement.SpanStart;
        }

        var commentLine = sourceText.Lines.GetLineFromPosition(
            firstDocumentingComment.Value.SpanStart);

        return commentLine.Start;
    }

    /// <summary>
    /// Determines the line-ending sequence that should be used when inserting
    /// the blank line.
    /// </summary>
    /// <param name="document">
    /// The document being fixed.
    /// </param>
    /// <param name="sourceText">
    /// The current source text of the document.
    /// </param>
    /// <param name="targetLineNumber">
    /// The zero-based line number at which the blank line will be inserted.
    /// </param>
    /// <param name="cancellationToken">
    /// A token used to cancel the asynchronous operation.
    /// </param>
    /// <returns>
    /// The CRLF, LF, or CR sequence appropriate for the document.
    /// </returns>
    /// <remarks>
    /// <para>
    /// Line endings are resolved using the closest available source evidence
    /// first. This is preferable to inspecting an arbitrary line elsewhere in
    /// the document, particularly if a file contains mixed line endings.
    /// </para>
    /// <para>
    /// If the document contains no usable line ending, the consumer's
    /// <c>end_of_line</c> analyzer configuration is consulted. Only when
    /// neither source text nor configuration establishes a convention is LF
    /// used as a deterministic final fallback.
    /// </para>
    /// <para>
    /// The host operating system is deliberately not consulted. Analyzer and
    /// code-fix output must remain reproducible regardless of whether the same
    /// repository is processed on Windows, Linux, or another platform.
    /// </para>
    /// </remarks>
    private static async Task<string> GetLineBreakAsync(
        Document document,
        SourceText sourceText,
        int targetLineNumber,
        CancellationToken cancellationToken)
    {
        // Prefer the line ending immediately preceding the target. This is the
        // strongest indication of the convention in use at the insertion point.
        if (targetLineNumber > 0)
        {
            var previousLine = sourceText.Lines[targetLineNumber - 1];
            var lineBreak = GetLineBreak(sourceText, previousLine);

            if (lineBreak is not null)
            {
                return lineBreak;
            }
        }

        // If there is no usable preceding line ending, inspect the target line.
        // This handles cases such as inserting near the beginning of a document.
        if (targetLineNumber < sourceText.Lines.Count)
        {
            var targetLine = sourceText.Lines[targetLineNumber];
            var lineBreak = GetLineBreak(sourceText, targetLine);

            if (lineBreak is not null)
            {
                return lineBreak;
            }
        }

        // A single-line document may contain no physical line ending from which
        // to infer a convention. In that case, consult Roslyn's analyzer config
        // options so an end_of_line setting from .editorconfig can be honoured.
        var syntaxTree = await document.GetSyntaxTreeAsync(
            cancellationToken).ConfigureAwait(false);

        if (syntaxTree is not null)
        {
            var options = document.Project.AnalyzerOptions
                .AnalyzerConfigOptionsProvider
                .GetOptions(syntaxTree);

            if (options.TryGetValue("end_of_line", out var endOfLine))
            {
                // Translate the EditorConfig value into the corresponding
                // physical character sequence required by SourceText.
                switch (endOfLine)
                {
                    case "crlf":
                        return "\r\n";

                    case "cr":
                        return "\r";

                    case "lf":
                        return "\n";
                }
            }
        }

        // At this point neither the source nor repository configuration defines
        // a newline convention. LF provides a deterministic fallback and,
        // unlike Environment.NewLine, cannot vary according to the host OS.
        return "\n";
    }

    /// <summary>
    /// Extracts the physical line-ending sequence from a source line.
    /// </summary>
    /// <param name="sourceText">
    /// The source text containing the line.
    /// </param>
    /// <param name="line">
    /// The line whose terminating characters should be inspected.
    /// </param>
    /// <returns>
    /// The line-ending characters, or <see langword="null"/> when the line has
    /// no terminating line break.
    /// </returns>
    /// <remarks>
    /// <see cref="TextLine.Span"/> excludes the line-ending characters while
    /// <see cref="TextLine.SpanIncludingLineBreak"/> includes them. The
    /// difference between those spans therefore represents exactly the
    /// document's original line-ending sequence.
    /// </remarks>
    private static string? GetLineBreak(
        SourceText sourceText,
        TextLine line)
    {
        // The final line of a document commonly has no terminating newline.
        // Treat that as "unknown" rather than inventing a newline convention.
        if (line.EndIncludingLineBreak == line.End)
        {
            return null;
        }

        // Extract the original characters verbatim so CRLF, LF, and CR are all
        // preserved without platform-specific assumptions.
        return sourceText.ToString(
            TextSpan.FromBounds(
                line.End,
                line.EndIncludingLineBreak));
    }
}