using CodeConform.CSharp.Analyzers;
using CodeConform.CSharp.CodeFixes;
using CodeConform.CSharp.Tests.TestInfrastructure;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;

namespace CodeConform.CSharp.Tests.CodeFixes;

/// <summary>
/// Verifies CC0002 edits, documenting-comment placement and batch behavior.
/// </summary>
[TestClass]
public sealed class BlankLineBeforeBlockStatementCodeFixProviderTests
{
    /// <summary>
    /// Verifies the ordinary CC0002 edit before an if statement.
    /// Expected result: one blank line is inserted before the if statement.
    /// </summary>
    [TestMethod]
    public async Task IfBlockAfterStatementIsFixed()
    {
        var test = CreateTest("IfAfterStatement.cs.txt", "IfAfterStatement.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 11, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that CC0002 inserts separation before a documenting comment.
    /// Expected result: the comment remains adjacent to its foreach statement.
    /// </summary>
    [TestMethod]
    public async Task CommentedForeachIsFixedBeforeComment()
    {
        var test = CreateTest(
            "CommentedLoopAfterStatement.cs.txt",
            "CommentedLoopAfterStatement.Fixed.cs.txt");

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(9, 9, 12, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies that Roslyn's batch fixer can correct independent CC0002
    /// violations in one document. Expected result: both missing separators
    /// appear in the final fixture.
    /// </summary>
    [TestMethod]
    public async Task MultipleBlockStatementsAreFixed()
    {
        var test = CreateTest(
            "MultipleBlockStatements.cs.txt",
            "MultipleBlockStatements.Fixed.cs.txt");
        test.NumberOfIncrementalIterations = 2;

        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(8, 9, 11, 10));
        test.ExpectedDiagnostics.Add(
            new DiagnosticResult("CC0002", DiagnosticSeverity.Warning)
                .WithSpan(12, 9, 15, 10));

        await test.RunAsync();
    }

    /// <summary>
    /// Verifies automatic correction for the complete supported statement
    /// family. Expected result: every missing separator is inserted while all
    /// statement bodies and grammatical continuation clauses are preserved.
    /// </summary>
    [TestMethod]
    public async Task SupportedBlockStatementKindsAreFixed()
    {
        var test = CreateTest("SupportedKinds.cs.txt", "SupportedKinds.Fixed.cs.txt");
        test.NumberOfIncrementalIterations = 8;

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
    /// Verifies Fix All with consecutive line comments and multiline block
    /// comments attached to all principal block-opening statements. Expected
    /// result: separators are inserted before the first comment of every group.
    /// </summary>
    [TestMethod]
    public async Task CommentedSupportedKindsAreFixedBeforeComments()
    {
        var test = CreateTest(
            "CommentedSupportedKinds.cs.txt",
            "CommentedSupportedKinds.Fixed.cs.txt");
        test.NumberOfIncrementalIterations = 9;

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

    private static CSharpCodeFixTest<
        BlankLineBeforeBlockStatementAnalyzer,
        BlankLineBeforeBlockStatementCodeFixProvider,
        DefaultVerifier> CreateTest(
        string sourceFixture,
        string fixedFixture)
    {
        return new CSharpCodeFixTest<
            BlankLineBeforeBlockStatementAnalyzer,
            BlankLineBeforeBlockStatementCodeFixProvider,
            DefaultVerifier>
        {
            TestCode = FixtureLoader.Load("BlankLineBeforeBlockStatement", sourceFixture),
            FixedCode = FixtureLoader.Load("BlankLineBeforeBlockStatement", fixedFixture)
        };
    }
}