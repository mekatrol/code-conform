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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterInvocation.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterAssignment.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterLocalDeclaration.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "VoidReturnAfterStatement.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterIfBlock.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
    /// Verifies that a return following a foreach block is corrected without
    /// modifying the loop body.
    /// Expected result: CC0001 is produced and a blank line is inserted after
    /// the foreach block.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterForeachBlockInsertsBlankLine()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterForeachBlock.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterForeachBlock.Fixed.cs.txt");

        var test = CreateTest(source, fixedSource);

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult(
                "CC0001",
                DiagnosticSeverity.Warning)
                .WithSpan(
                    10,
                    9,
                    10,
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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterTryCatch.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "MultilineReturnAfterStatement.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterCommentWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterMultipleCommentsWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "ReturnAfterMultilineCommentWithoutBlankLine.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
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
            "BlankLineBeforeReturnCodeFix",
            "MultipleInvalidReturns.cs.txt");

        var fixedSource = FixtureLoader.Load(
            "BlankLineBeforeReturnCodeFix",
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