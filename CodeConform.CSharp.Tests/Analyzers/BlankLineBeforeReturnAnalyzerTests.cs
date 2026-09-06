using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.Tests.TestInfrastructure;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.Analyzers;

/// <summary>
/// Tests the <see cref="BlankLineBeforeReturnAnalyzer"/> rule that requires
/// a blank line before a return statement unless the return statement is the
/// first statement in its containing block.
/// </summary>
[TestClass]
public sealed class BlankLineBeforeReturnAnalyzerTests
{
    /// <summary>
    /// Verifies that a return statement that is the first statement in a block
    /// is exempt from the blank-line requirement.
    /// Expected result: no CC0001 diagnostic is produced.
    /// </summary>
    [TestMethod]
    public async Task ReturnAsFirstStatementDoesNotReportDiagnostic()
    {
        await VerifyNoDiagnosticAsync(
            "ReturnAsFirstStatement.cs.txt");
    }

    /// <summary>
    /// Verifies that a return statement separated from the preceding statement
    /// by a blank line satisfies the formatting rule.
    /// Expected result: no CC0001 diagnostic is produced.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterBlankLineDoesNotReportDiagnostic()
    {
        await VerifyNoDiagnosticAsync(
            "ReturnAfterBlankLine.cs.txt");
    }

    /// <summary>
    /// Verifies that a return statement immediately following a method
    /// invocation violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterInvocationReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterInvocation.cs.txt",
            6,
            9,
            6,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following an assignment
    /// violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterAssignmentReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterAssignment.cs.txt",
            7,
            9,
            7,
            22);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a local variable
    /// declaration violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterLocalDeclarationReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterLocalDeclaration.cs.txt",
            6,
            9,
            6,
            22);
    }

    /// <summary>
    /// Verifies that a return statement immediately following an if block
    /// violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterIfBlockReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterIfBlock.cs.txt",
            9,
            9,
            9,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a for block
    /// violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterForBlockReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterForBlock.cs.txt",
            9,
            9,
            9,
            19);
    }

    /// <summary>
    /// Verifies that return statements immediately following foreach blocks
    /// violate the blank-line requirement.
    /// Expected result: two CC0001 diagnostics are produced, one for each
    /// return statement that immediately follows a foreach block.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterForeachBlockReportsDiagnostics()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "ReturnAfterForeachBlock.cs.txt");

        var test = CreateTest(source);

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
    /// Verifies that a return statement immediately following a while block
    /// violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterWhileBlockReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterWhileBlock.cs.txt",
            9,
            9,
            9,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a do-while
    /// statement violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterDoWhileBlockReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterDoWhileBlock.cs.txt",
            10,
            9,
            10,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a using block
    /// violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterUsingBlockReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterUsingBlock.cs.txt",
            11,
            9,
            11,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a lock block
    /// violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterLockBlockReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterLockBlock.cs.txt",
            11,
            9,
            11,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a try/catch
    /// statement violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterTryCatchReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterTryCatch.cs.txt",
            13,
            9,
            13,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a switch
    /// statement violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterSwitchReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterSwitch.cs.txt",
            13,
            9,
            13,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following a local function
    /// declaration violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterLocalFunctionReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterLocalFunction.cs.txt",
            8,
            9,
            8,
            19);
    }

    /// <summary>
    /// Verifies that a return statement immediately following an expression
    /// statement violates the blank-line requirement.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterExpressionStatementReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterExpressionStatement.cs.txt",
            6,
            9,
            6,
            19);
    }

    /// <summary>
    /// Verifies that a comment immediately documenting a return statement does
    /// not remove the requirement for separation from the preceding statement.
    /// Expected result: CC0001 is produced because the required blank line is
    /// missing before the comment and return group.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterCommentWithoutBlankLineReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterCommentWithoutBlankLine.cs.txt",
            7,
            9,
            7,
            19);
    }

    /// <summary>
    /// Verifies that a multiline comment immediately documenting a return
    /// statement does not remove the requirement for separation from the
    /// preceding statement.
    /// Expected result: CC0001 is produced because the required blank line is
    /// missing before the comment and return group.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterMultilineCommentWithoutBlankLineReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterMultilineCommentWithoutBlankLine.cs.txt",
            9,
            9,
            9,
            19);
    }

    /// <summary>
    /// Verifies that multiple contiguous comments documenting a return statement
    /// are treated as part of the return statement's comment group.
    /// Expected result: CC0001 is produced because the required blank line is
    /// missing before the comment group.
    /// </summary>
    [TestMethod]
    public async Task ReturnAfterMultipleCommentsWithoutBlankLineReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "ReturnAfterMultipleCommentsWithoutBlankLine.cs.txt",
            8,
            9,
            8,
            19);
    }

    /// <summary>
    /// Verifies that a return statement without an expression is subject to the
    /// same blank-line requirement as a return statement with an expression.
    /// Expected result: CC0001 is produced for the return statement.
    /// </summary>
    [TestMethod]
    public async Task VoidReturnAfterStatementReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "VoidReturnAfterStatement.cs.txt",
            6,
            9,
            6,
            16);
    }

    /// <summary>
    /// Verifies that a multiline return statement is subject to the same
    /// blank-line requirement and that the diagnostic spans the complete
    /// multiline return statement.
    /// Expected result: CC0001 is produced for the outer return statement only;
    /// the return that is first in its own method block remains valid.
    /// </summary>
    [TestMethod]
    public async Task MultilineReturnAfterStatementReportsDiagnostic()
    {
        await VerifyDiagnosticAsync(
            "MultilineReturnAfterStatement.cs.txt",
            6,
            9,
            8,
            16);
    }

    /// <summary>
    /// Verifies that each independently invalid return statement is diagnosed
    /// when more than one violation exists in the same source file.
    /// Expected result: two CC0001 diagnostics are produced, one for each
    /// improperly separated return statement.
    /// </summary>
    [TestMethod]
    public async Task MultipleInvalidReturnsReportMultipleDiagnostics()
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            "MultipleInvalidReturns.cs.txt");

        var test = CreateTest(source);

        test.ExpectedDiagnostics.Add(
            CreateExpectedDiagnostic(
                10,
                13,
                10,
                26));

        test.ExpectedDiagnostics.Add(
            CreateExpectedDiagnostic(
                14,
                9,
                14,
                22));

        await test.RunAsync();
    }

    private static async Task VerifyNoDiagnosticAsync(
        string fixtureName)
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            fixtureName);

        var test = CreateTest(source);

        await test.RunAsync();
    }

    private static async Task VerifyDiagnosticAsync(
        string fixtureName,
        int startLine,
        int startColumn,
        int endLine,
        int endColumn)
    {
        var source = FixtureLoader.Load(
            "BlankLineBeforeReturn",
            fixtureName);

        var test = CreateTest(source);

        test.ExpectedDiagnostics.Add(
            CreateExpectedDiagnostic(
                startLine,
                startColumn,
                endLine,
                endColumn));

        await test.RunAsync();
    }

    private static CSharpAnalyzerTest<BlankLineBeforeReturnAnalyzer, DefaultVerifier>
        CreateTest(string source)
    {
        return new CSharpAnalyzerTest<BlankLineBeforeReturnAnalyzer, DefaultVerifier>
        {
            TestCode = source
        };
    }

    private static DiagnosticResult CreateExpectedDiagnostic(
        int startLine,
        int startColumn,
        int endLine,
        int endColumn)
    {
        return new DiagnosticResult(
            "CC0001",
            DiagnosticSeverity.Warning)
            .WithSpan(
                startLine,
                startColumn,
                endLine,
                endColumn);
    }
}