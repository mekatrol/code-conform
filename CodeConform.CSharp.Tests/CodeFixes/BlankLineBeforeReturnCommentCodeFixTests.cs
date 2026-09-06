using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.CodeFixes;
using CodeConform.CSharp.Tests.TestInfrastructure;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.CodeFixes;

/// <summary>
/// Tests comment-block placement behavior for
/// <see cref="BlankLineBeforeReturnCodeFixProvider"/>.
/// These tests verify that CC0001 inserts the required separator before a
/// contiguous documenting comment block rather than between the comment block
/// and the return statement.
/// </summary>
[TestClass]
public sealed class BlankLineBeforeReturnCommentCodeFixTests
{
    /// <summary>
    /// Verifies that a single-line comment immediately preceding a return is
    /// treated as documentation for that return.
    /// Expected result: CC0001 is produced and the blank line is inserted
    /// before the comment, leaving the comment adjacent to the return.
    /// </summary>
    [TestMethod]
    public async Task SingleLineCommentBeforeReturnMovesSeparatorBeforeComment()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterSingleLineCommentWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterSingleLineCommentWithoutBlankLine.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    7,
                    9,
                    7,
                    16));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that multiple consecutive single-line comments immediately
    /// preceding a return are treated as one contiguous documenting block.
    /// Expected result: CC0001 is produced and the blank line is inserted
    /// before the first comment in the block.
    /// </summary>
    [TestMethod]
    public async Task MultipleSingleLineCommentsBeforeReturnMoveSeparatorBeforeCommentBlock()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterMultipleSingleLineCommentsWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterMultipleSingleLineCommentsWithoutBlankLine.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    8,
                    9,
                    8,
                    16));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a multiline block comment immediately preceding a return
    /// is treated as documentation for that return.
    /// Expected result: CC0001 is produced and the blank line is inserted
    /// before the opening delimiter of the block comment.
    /// </summary>
    [TestMethod]
    public async Task BlockCommentBeforeReturnMovesSeparatorBeforeCommentBlock()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterBlockCommentWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterBlockCommentWithoutBlankLine.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    9,
                    9,
                    9,
                    16));

        await test.RunAsync();
    }

    /// <summary>
    /// Creates a Roslyn analyzer/code-fix test for the supplied source and
    /// exact expected fixed source.
    /// </summary>
    private static CSharpCodeFixTest<
        BlankLineBeforeReturnAnalyzer,
        BlankLineBeforeReturnCodeFixProvider,
        DefaultVerifier> CreateTest(
            string source,
            string fixedSource)
    {
        return new CSharpCodeFixTest<
            BlankLineBeforeReturnAnalyzer,
            BlankLineBeforeReturnCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = source,
            FixedCode = fixedSource
        };
    }
}