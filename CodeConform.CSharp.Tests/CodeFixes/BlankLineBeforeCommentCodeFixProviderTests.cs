using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.CodeFixes;
using CodeConform.CSharp.Tests.TestInfrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.CodeFixes;

/// <summary>
/// Verifies CC0004 edits for individual and grouped ordinary comments.
/// </summary>
[TestClass]
public sealed class BlankLineBeforeCommentCodeFixProviderTests
{
    /// <summary>
    /// Verifies the standard edit before a line comment.
    /// Expected result: one blank line precedes the comment and following code.
    /// </summary>
    [TestMethod]
    public async Task CommentAfterStatementIsFixed()
    {
        var test = CreateTest("CommentAfterStatement.cs.txt", "CommentAfterStatement.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 8, 39));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that one edit separates an entire mixed comment block.
    /// Expected result: the line and block comments remain mutually adjacent.
    /// </summary>
    [TestMethod]
    public async Task MixedCommentBlockIsFixedAsOneGroup()
    {
        var test = CreateTest("MixedCommentBlock.cs.txt", "MixedCommentBlock.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 8, 30));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a consecutive line-comment block receives one separator.
    /// Expected result: all three comment lines remain contiguous after fixing.
    /// </summary>
    [TestMethod]
    public async Task ConsecutiveLineCommentsAreFixedAsOneGroup()
    {
        var test = CreateTest(
            "ConsecutiveLineComments.cs.txt",
            "ConsecutiveLineComments.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 8, 42));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the fix is inserted before a complete multiline block
    /// comment. Expected result: indentation and interior comment layout remain
    /// unchanged while one blank line precedes the opening delimiter.
    /// </summary>
    [TestMethod]
    public async Task MultilineBlockCommentIsFixedAsOneGroup()
    {
        var test = CreateTest(
            "MultilineBlockComment.cs.txt",
            "MultilineBlockComment.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 11, 12));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies the shared-boundary edit when comments follow a closing brace.
    /// Expected result: the separator is inserted before the first comment and
    /// not between the comments or between the comments and following code.
    /// </summary>
    [TestMethod]
    public async Task CommentAfterClosingBraceIsFixedBeforeCommentBlock()
    {
        var test = CreateTest(
            "CommentAfterClosingBrace.cs.txt",
            "CommentAfterClosingBrace.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0004", DiagnosticSeverity.Warning)
                .WithSpan(11, 9, 11, 51));

        await test.RunAsync();
    }

    private static CSharpCodeFixTest<
        BlankLineBeforeCommentAnalyzer,
        BlankLineBeforeCommentCodeFixProvider,
        DefaultVerifier> CreateTest(
        string sourceFixture,
        string fixedFixture)
    {
        return new CSharpCodeFixTest<
            BlankLineBeforeCommentAnalyzer,
            BlankLineBeforeCommentCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = FixtureLoader.Load("BlankLineBeforeComment", sourceFixture),
            FixedCode = FixtureLoader.Load("BlankLineBeforeComment", fixedFixture)
        };
    }
}