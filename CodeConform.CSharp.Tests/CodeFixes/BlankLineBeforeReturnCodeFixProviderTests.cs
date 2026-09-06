using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.CodeFixes;
using CodeConform.CSharp.Tests.TestInfrastructure;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.CodeFixes;

/// <summary>
/// Tests <see cref="BlankLineBeforeReturnCodeFixProvider"/> together with
/// <see cref="BlankLineBeforeReturnAnalyzer"/>.
/// The suite verifies that CC0001 violations are corrected to the exact
/// expected source text and that already-conforming source remains unchanged.
/// </summary>
[TestClass]
public sealed class BlankLineBeforeReturnCodeFixProviderTests
{
    /// <summary>
    /// Verifies that a return statement immediately following an invocation
    /// is diagnosed and fixed by inserting one blank line before the return.
    /// Expected result: CC0001 is produced and the fixed source matches the
    /// corresponding fixed fixture exactly.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterInvocationInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterInvocation.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterInvocation.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    6,
                    9,
                    6,
                    19));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a return statement immediately following an assignment
    /// is corrected without altering the assignment or return expression.
    /// Expected result: CC0001 is produced and exactly one blank line is added.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterAssignmentInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterAssignment.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterAssignment.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    7,
                    9,
                    7,
                    22));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a return statement immediately following a local
    /// declaration is corrected.
    /// Expected result: CC0001 is produced and a blank line separates the
    /// declaration from the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterLocalDeclarationInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterLocalDeclaration.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterLocalDeclaration.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    6,
                    9,
                    6,
                    22));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that the rule and fixer also apply to a bare return in a void
    /// method rather than only to returns containing an expression.
    /// Expected result: CC0001 is produced and a blank line is inserted before
    /// the bare return statement.
    /// </summary>
    [TestMethod]
    public async Task VoidReturnAfterStatementInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "VoidReturnAfterStatement.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "VoidReturnAfterStatement.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    6,
                    9,
                    6,
                    16));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a return following an if block is separated from the
    /// closing brace of that block.
    /// Expected result: CC0001 is produced and the fixed source contains a
    /// blank line between the closing brace and return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterIfBlockInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterIfBlock.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterIfBlock.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    9,
                    9,
                    9,
                    19));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that return statements following foreach blocks are corrected
    /// without modifying either loop body.
    /// Expected result: two CC0001 diagnostics are produced and a blank line is
    /// inserted after each foreach block before the corresponding return.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterForeachBlockInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterForeachBlock.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterForeachBlock.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    9,
                    9,
                    9,
                    19));

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    23,
                    9,
                    23,
                    22));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a return following a try/catch statement is corrected
    /// after the complete try/catch construct rather than inside either block.
    /// Expected result: CC0001 is produced and the blank line is inserted
    /// between the catch block and return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterTryCatchInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterTryCatch.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterTryCatch.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    13,
                    9,
                    13,
                    19));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a multiline return expression is treated as one return
    /// statement and that the fix is inserted before its first line.
    /// Expected result: CC0001 spans the complete multiline return and the
    /// return expression itself remains unchanged.
    /// </summary>
    [TestMethod]
    public async Task MultilineReturnAfterStatementInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "MultilineReturnAfterStatement.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "MultilineReturnAfterStatement.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    6,
                    9,
                    8,
                    16));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies the documenting-comment rule for a single line comment that
    /// immediately precedes the return statement.
    /// Expected result: CC0001 is produced and the blank line is inserted
    /// before the documenting comment, not between the comment and return.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterSingleCommentMovesSeparatorBeforeComment()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterCommentWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterCommentWithoutBlankLine.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    7,
                    9,
                    7,
                    19));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies the documenting-comment rule for multiple consecutive
    /// single-line comments immediately preceding the return.
    /// Expected result: CC0001 is produced and the blank line is inserted
    /// before the entire contiguous comment block.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterMultipleCommentsMovesSeparatorBeforeCommentBlock()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterMultipleCommentsWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterMultipleCommentsWithoutBlankLine.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    8,
                    9,
                    8,
                    19));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies the documenting-comment rule for a multiline block comment
    /// immediately preceding the return statement.
    /// Expected result: CC0001 is produced and the blank line is inserted
    /// before the block comment rather than inside or after it.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterMultilineCommentMovesSeparatorBeforeCommentBlock()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterMultilineCommentWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterMultilineCommentWithoutBlankLine.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    9,
                    9,
                    9,
                    19));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that an already-correct return separated by a blank line does
    /// not cause the analyzer or code-fix provider to make any change.
    /// Expected result: no CC0001 diagnostic is produced and the source is
    /// accepted unchanged.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterExistingBlankLineDoesNotOfferFix()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterBlankLine.cs.txt");

        var test = CreateTest(source);

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a return which is the first statement in its containing
    /// block is exempt from the blank-line requirement.
    /// Expected result: no CC0001 diagnostic is produced and no fix is needed.
    /// </summary>
    [TestMethod]
    public async Task ReturnAsFirstStatementDoesNotOfferFix()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAsFirstStatement.cs.txt");

        var test = CreateTest(source);

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that a documenting comment preceding the first return
    /// statement does not change the first-statement exemption.
    /// Expected result: no CC0001 diagnostic is produced because comments are
    /// trivia rather than statements and the return remains first in the block.
    /// </summary>
    [TestMethod]
    public async Task ReturnAsFirstStatementAfterCommentDoesNotOfferFix()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAsFirstStatementAfterComment.cs.txt");

        var test = CreateTest(source);

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that multiple independent CC0001 violations in one document
    /// can all be corrected to the expected final source.
    /// Expected result: two CC0001 diagnostics are produced and both returns
    /// are separated from their preceding statements by blank lines.
    /// </summary>
    [TestMethod]
    public async Task MultipleInvalidReturnsAreAllFixed()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "MultipleInvalidReturns.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "MultipleInvalidReturns.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    10,
                    13,
                    10,
                    26));

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    14,
                    9,
                    14,
                    22));

        await test.RunAsync();
    }

    /// <summary>
    /// Creates a Roslyn analyzer/code-fix test for conforming source where no
    /// diagnostic and therefore no fixed document are expected.
    /// </summary>
    /// <param name="source">
    /// The source text to analyze.
    /// </param>
    /// <returns>
    /// A configured Roslyn code-fix test.
    /// </returns>
    private static CSharpCodeFixTest<
        BlankLineBeforeReturnAnalyzer,
        BlankLineBeforeReturnCodeFixProvider,
        DefaultVerifier> CreateTest(
            string source)
    {
        return new CSharpCodeFixTest<
            BlankLineBeforeReturnAnalyzer,
            BlankLineBeforeReturnCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = source
        };
    }

    /// <summary>
    /// Creates a Roslyn analyzer/code-fix test for source containing CC0001
    /// violations and the exact source expected after fixes are applied.
    /// </summary>
    /// <param name="source">
    /// The source text containing the violation.
    /// </param>
    /// <param name="fixedSource">
    /// The exact source text expected after the code fix is applied.
    /// </param>
    /// <returns>
    /// A configured Roslyn code-fix test.
    /// </returns>
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