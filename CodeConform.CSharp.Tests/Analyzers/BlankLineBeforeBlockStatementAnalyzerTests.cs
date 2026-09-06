using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.Tests.TestInfrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.Analyzers;

/// <summary>
/// Specifies the CC0002 rule for executable statements that open blocks,
/// including first-statement, comment-association and excluded-syntax behavior.
/// </summary>
[TestClass]
public sealed class BlankLineBeforeBlockStatementAnalyzerTests
{
    /// <summary>
    /// Verifies that an if block following another statement requires separation.
    /// Expected result: CC0002 spans the complete if statement.
    /// </summary>
    [TestMethod]
    public async Task IfBlockAfterStatementReportsDiagnostic()
    {
        var test = CreateTest("IfAfterStatement.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 11, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that comments immediately documenting a loop remain part of
    /// the loop's logical group. Expected result: CC0002 is reported for the
    /// foreach statement rather than CC0004 being needed for its comment.
    /// </summary>
    [TestMethod]
    public async Task CommentedForeachAfterStatementReportsDiagnostic()
    {
        var test = CreateTest("CommentedLoopAfterStatement.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(9, 9, 12, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that every independent invalid block statement is analyzed.
    /// Expected result: CC0002 is reported for both the while and lock blocks.
    /// </summary>
    [TestMethod]
    public async Task MultipleBlockStatementsReportDiagnostics()
    {
        var test = CreateTest("MultipleBlockStatements.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 11, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(12, 9, 15, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies the complete family of supported block-opening constructs in
    /// one specification fixture. Expected result: CC0002 is reported for for,
    /// foreach, while, do, switch, try, using and lock statements.
    /// </summary>
    [TestMethod]
    public async Task SupportedBlockStatementKindsReportDiagnostics()
    {
        var test = CreateTest("SupportedKinds.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 10, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(13, 9, 16, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(19, 9, 22, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(25, 9, 29, 23));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(32, 9, 36, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(39, 9, 45, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(48, 9, 51, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(54, 9, 57, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies comment association across every principal CC0002 construct
    /// using both consecutive line comments and multiline block comments.
    /// Expected result: nine CC0002 diagnostics span the statements, while the
    /// comments remain owned by their respective statements.
    /// </summary>
    [TestMethod]
    public async Task CommentedSupportedKindsReportDiagnostics()
    {
        var test = CreateTest("CommentedSupportedKinds.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(10, 9, 13, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(19, 9, 22, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(27, 9, 30, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(37, 9, 40, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(45, 9, 49, 23));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(55, 9, 59, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(64, 9, 70, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(76, 9, 79, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(84, 9, 87, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that both line-comment and multiline block-comment groups
    /// preserve the first-statement exemption of the documented statement.
    /// Expected result: no CC0002 diagnostic is produced.
    /// </summary>
    [TestMethod]
    public async Task FirstStatementCommentBlocksDoNotReportDiagnostic()
    {
        var test = CreateTest("FirstStatementCommentBlocks.cs.txt");

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that existing separators before consecutive line comments and
    /// multiline block comments satisfy CC0002. Expected result: no CC0002
    /// diagnostic is produced for either documented statement.
    /// </summary>
    [TestMethod]
    public async Task SeparatedCommentBlocksDoNotReportDiagnostic()
    {
        var test = CreateTest("SeparatedCommentBlocks.cs.txt");

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies the intentional exclusions for a first statement, an else-if
    /// continuation, an unbraced if and a using declaration.
    /// Expected result: no CC0002 diagnostic is produced.
    /// </summary>
    [TestMethod]
    public async Task ExcludedAndFirstStatementsDoNotReportDiagnostic()
    {
        var test = CreateTest("ExcludedAndFirstStatements.cs.txt");

        await test.RunAsync();
    }

    private static CSharpAnalyzerTest<
        BlankLineBeforeBlockStatementAnalyzer,
        DefaultVerifier> CreateTest(string fixtureName)
    {
        return new CSharpAnalyzerTest<
            BlankLineBeforeBlockStatementAnalyzer,
            DefaultVerifier>
        {
            TestCode = FixtureLoader.Load(
                "BlankLineBeforeBlockStatement",
                fixtureName)
        };
    }
}