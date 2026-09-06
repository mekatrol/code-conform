using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodeConform.CSharp.Analyzers.Syntax;

/// <summary>
/// Provides shared syntax and source-boundary classification for formatting rules.
/// </summary>
internal static class FormattingSyntax
{
    /// <summary>
    /// Determines whether a source interval contains an empty physical line.
    /// </summary>
    public static bool ContainsBlankLine(
        SourceText sourceText,
        int precedingPosition,
        int followingPosition)
    {
        var precedingLine = sourceText.Lines.GetLineFromPosition(precedingPosition).LineNumber;
        var followingLine = sourceText.Lines.GetLineFromPosition(followingPosition).LineNumber;

        for (var lineNumber = precedingLine + 1;
            lineNumber < followingLine;
            lineNumber++)
        {
            if (string.IsNullOrWhiteSpace(sourceText.Lines[lineNumber].ToString()))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the start of the contiguous comment block documenting a node.
    /// </summary>
    public static int GetDocumentedGroupStart(SyntaxNode node)
    {
        var groupStart = node.SpanStart;
        var trivia = node.GetLeadingTrivia();

        for (var index = trivia.Count - 1; index >= 0; index--)
        {
            var item = trivia[index];

            if (item.IsKind(SyntaxKind.WhitespaceTrivia) ||
                item.IsKind(SyntaxKind.EndOfLineTrivia))
            {
                continue;
            }

            if (IsOrdinaryComment(item))
            {
                groupStart = item.SpanStart;

                continue;
            }

            break;
        }

        return groupStart;
    }

    /// <summary>
    /// Determines whether trivia is an ordinary non-documentation comment.
    /// </summary>
    public static bool IsOrdinaryComment(SyntaxTrivia trivia)
    {
        return trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
            trivia.IsKind(SyntaxKind.MultiLineCommentTrivia);
    }

    /// <summary>
    /// Determines whether a statement is one of the supported block-opening forms.
    /// </summary>
    public static bool IsBlockOpeningStatement(StatementSyntax statement)
    {
        return statement switch
        {
            IfStatementSyntax ifStatement =>
                ifStatement.Statement is BlockSyntax ||
                ifStatement.Else?.Statement is BlockSyntax,
            ForStatementSyntax forStatement => forStatement.Statement is BlockSyntax,
            ForEachStatementSyntax forEachStatement => forEachStatement.Statement is BlockSyntax,
            ForEachVariableStatementSyntax forEachVariableStatement =>
                forEachVariableStatement.Statement is BlockSyntax,
            WhileStatementSyntax whileStatement => whileStatement.Statement is BlockSyntax,
            DoStatementSyntax doStatement => doStatement.Statement is BlockSyntax,
            SwitchStatementSyntax => true,
            TryStatementSyntax => true,
            UsingStatementSyntax usingStatement => usingStatement.Statement is BlockSyntax,
            LockStatementSyntax lockStatement => lockStatement.Statement is BlockSyntax,
            FixedStatementSyntax fixedStatement => fixedStatement.Statement is BlockSyntax,
            _ => false
        };
    }

    /// <summary>
    /// Finds the previous statement in the same executable statement list.
    /// </summary>
    public static StatementSyntax? GetPreviousStatement(StatementSyntax statement)
    {
        SyntaxList<StatementSyntax> statements;

        if (statement.Parent is BlockSyntax block)
        {
            statements = block.Statements;
        }
        else if (statement.Parent is SwitchSectionSyntax section)
        {
            statements = section.Statements;
        }
        else
        {
            return null;
        }

        var index = statements.IndexOf(statement);

        return index > 0 ? statements[index - 1] : null;
    }

    /// <summary>
    /// Determines whether a closing brace belongs to a formatting-relevant body.
    /// </summary>
    public static bool IsSemanticClosingBrace(SyntaxToken token)
    {
        if (!token.IsKind(SyntaxKind.CloseBraceToken))
        {
            return false;
        }

        if (token.Parent is BlockSyntax block)
        {
            return block.Parent is not AnonymousFunctionExpressionSyntax;
        }

        return token.Parent is SwitchStatementSyntax ||
            token.Parent is BaseNamespaceDeclarationSyntax ||
            token.Parent is BaseTypeDeclarationSyntax ||
            token.Parent is AccessorListSyntax;
    }

    /// <summary>
    /// Determines whether the next token is a grammatical continuation of a brace.
    /// </summary>
    public static bool IsClosingBraceContinuation(
        SyntaxToken closingBrace,
        SyntaxToken nextToken)
    {
        if (nextToken.RawKind == 0 ||
            nextToken.IsKind(SyntaxKind.CloseBraceToken) ||
            nextToken.IsKind(SyntaxKind.SemicolonToken) ||
            nextToken.IsKind(SyntaxKind.ElseKeyword) ||
            nextToken.IsKind(SyntaxKind.CatchKeyword) ||
            nextToken.IsKind(SyntaxKind.FinallyKeyword))
        {
            return true;
        }

        return nextToken.IsKind(SyntaxKind.WhileKeyword) &&
            closingBrace.Parent?.Parent is DoStatementSyntax;
    }
}