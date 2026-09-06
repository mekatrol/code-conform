using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeConform.CSharp.CodeFixes;

/// <summary>
/// Provides shared, newline-preserving edits for blank-line code fixes.
/// </summary>
internal static class FormattingCodeFix
{
    /// <summary>
    /// Inserts one physical blank line before the source line containing a position.
    /// </summary>
    public static async Task<Document> InsertBlankLineAsync(
        Document document,
        int position,
        CancellationToken cancellationToken)
    {
        var sourceText = await document.GetTextAsync(cancellationToken).ConfigureAwait(false);
        var targetLine = sourceText.Lines.GetLineFromPosition(position);
        var lineBreak = await GetLineBreakAsync(
            document,
            sourceText,
            targetLine.LineNumber,
            cancellationToken).ConfigureAwait(false);

        return document.WithText(
            sourceText.WithChanges(
                new TextChange(
                    new TextSpan(targetLine.Start, 0),
                    lineBreak)));
    }

    /// <summary>
    /// Gets the first ordinary comment in the contiguous leading group for a node.
    /// </summary>
    public static int GetDocumentedGroupStart(SyntaxNode node)
    {
        var position = node.SpanStart;
        var trivia = node.GetLeadingTrivia();

        for (var index = trivia.Count - 1; index >= 0; index--)
        {
            var item = trivia[index];

            if (item.IsKind(SyntaxKind.WhitespaceTrivia) ||
                item.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            if (item.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
                item.IsKind(SyntaxKind.MultiLineCommentTrivia))
            {
                position = item.SpanStart;

                continue;
            }

            break;
        }

        return position;
    }

    private static async Task<string> GetLineBreakAsync(
        Document document,
        SourceText sourceText,
        int targetLineNumber,
        CancellationToken cancellationToken)
    {
        if (targetLineNumber > 0)
        {
            var lineBreak = GetLineBreak(sourceText, sourceText.Lines[targetLineNumber - 1]);

            if (lineBreak is not null)
            {
                return lineBreak;
            }
        }

        if (targetLineNumber < sourceText.Lines.Count)
        {
            var lineBreak = GetLineBreak(sourceText, sourceText.Lines[targetLineNumber]);

            if (lineBreak is not null)
            {
                return lineBreak;
            }
        }

        var syntaxTree = await document.GetSyntaxTreeAsync(cancellationToken).ConfigureAwait(false);

        if (syntaxTree is not null)
        {
            var options = document.Project.AnalyzerOptions
                .AnalyzerConfigOptionsProvider
                .GetOptions(syntaxTree);

            if (options.TryGetValue("end_of_line", out var endOfLine))
            {
                return endOfLine switch
                {
                    "crlf" => "\r\n",
                    "cr" => "\r",
                    _ => "\n"
                };
            }
        }

        return "\n";
    }

    private static string? GetLineBreak(SourceText sourceText, TextLine line)
    {
        if (line.End == line.EndIncludingLineBreak)
        {
            return null;
        }

        return sourceText.ToString(
            TextSpan.FromBounds(
                line.End,
                line.EndIncludingLineBreak));
    }
}